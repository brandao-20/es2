using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAPI.Context;
using WebAPI.Entities;
using WebAPI.Helpers;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilizadoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtilizadoresController(AppDbContext context)
        {
            _context = context;
        }

        // Endpoint de registro para usuário comum
        [HttpPost("register")]
        public async Task<ActionResult<Utilizador>> Register(Utilizador utilizador)
        {
            if (!_context.Utilizadores.Any())
            {
                var adminTipo = await _context.TipoUtilizadors.FirstOrDefaultAsync(t => t.Tipo.ToLower() == "admin");
                if (adminTipo == null)
                {
                    adminTipo = new TipoUtilizador { Tipo = "Admin" };
                    _context.TipoUtilizadors.Add(adminTipo);
                    await _context.SaveChangesAsync();
                }
                utilizador.TipoUtilizadorId = adminTipo.TipoUtilizadorId;
            }
            else
            {
                var userTipo = await _context.TipoUtilizadors.FirstOrDefaultAsync(t => t.Tipo.ToLower() == "user");
                if (userTipo == null)
                {
                    userTipo = new TipoUtilizador { Tipo = "User" };
                    _context.TipoUtilizadors.Add(userTipo);
                    await _context.SaveChangesAsync();
                }
                utilizador.TipoUtilizadorId = userTipo.TipoUtilizadorId;
            }

            utilizador.Password = PasswordHelper.HashPassword(utilizador.Password);
            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = utilizador.UtilizadorId }, utilizador);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Utilizador>> GetById(int id)
        {
            var user = await _context.Utilizadores.Include(u => u.TipoUtilizador)
                .FirstOrDefaultAsync(u => u.UtilizadorId == id);
            if (user == null) return NotFound();
            return user;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetAll()
        {
            return await _context.Utilizadores.Include(u => u.TipoUtilizador).ToListAsync();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Utilizador utilizador)
        {
            if (id != utilizador.UtilizadorId) return BadRequest();

            var existingUser = await _context.Utilizadores.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UtilizadorId == id);
            if (existingUser != null && utilizador.Password != existingUser.Password)
            {
                utilizador.Password = PasswordHelper.HashPassword(utilizador.Password);
            }

            _context.Entry(utilizador).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Utilizadores.Any(e => e.UtilizadorId == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst("utilizadorId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId) && id == currentUserId)
            {
                return BadRequest("Não é possível remover a si próprio por este endpoint.");
            }

            var user = await _context.Utilizadores.FindAsync(id);
            if (user == null) return NotFound();

            _context.Utilizadores.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Novo endpoint: usuário removendo sua própria conta
        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userIdClaim = User.FindFirst("utilizadorId");
            if (userIdClaim == null) return Unauthorized("Usuário não identificado.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("ID do usuário inválido.");

            var user = await _context.Utilizadores.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Utilizadores.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ================  NOVOS ENDPOINTS PARA ADMIN / MANAGER  =============

        // (1) AdminCreate: cria usuário com cargo (role) escolhido
        [HttpPost("admincreate")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> AdminCreate(UserDto dto)
        {
            // Converte "Admin", "UserManager", "User" -> "ADMIN", "USER_MANAGER", "USER"
            var roleDb = dto.Tipo.ToUpper();
            var tipoEntity = await _context.TipoUtilizadors.FirstOrDefaultAsync(t => t.Tipo == roleDb);
            if (tipoEntity == null)
            {
                tipoEntity = new TipoUtilizador { Tipo = roleDb };
                _context.TipoUtilizadors.Add(tipoEntity);
                await _context.SaveChangesAsync();
            }

            var user = new Utilizador
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = PasswordHelper.HashPassword(dto.Password),
                TipoUtilizadorId = tipoEntity.TipoUtilizadorId,
                DataCriacao = DateTime.UtcNow
            };

            _context.Utilizadores.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        // (2) AdminGetUser: retorna DTO para edição
        [HttpGet("adminget/{id:int}")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<ActionResult<UserDto>> AdminGetUser(int id)
        {
            var user = await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .FirstOrDefaultAsync(u => u.UtilizadorId == id);
            if (user == null) return NotFound();

            var roleDb = user.TipoUtilizador?.Tipo ?? "USER";
            var dto = new UserDto
            {
                UtilizadorId = user.UtilizadorId,
                Username = user.Username,
                Email = user.Email,
                // Normalizar "ADMIN" -> "Admin", etc.
                Tipo = NormalizeRoleFromDb(roleDb)
            };
            return dto;
        }

        // (3) AdminEditUser: edita username, email, cargo e opcionalmente password
        [HttpPut("adminedit/{id:int}")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> AdminEditUser(int id, UserDto dto)
        {
            var user = await _context.Utilizadores.FindAsync(id);
            if (user == null) return NotFound();

            user.Username = dto.Username;
            user.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                user.Password = PasswordHelper.HashPassword(dto.NewPassword);
            }

            var roleDb = dto.Tipo.ToUpper();
            var tipoEntity = await _context.TipoUtilizadors.FirstOrDefaultAsync(t => t.Tipo == roleDb);
            if (tipoEntity == null)
            {
                tipoEntity = new TipoUtilizador { Tipo = roleDb };
                _context.TipoUtilizadors.Add(tipoEntity);
                await _context.SaveChangesAsync();
            }
            user.TipoUtilizadorId = tipoEntity.TipoUtilizadorId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private string NormalizeRoleFromDb(string roleDb)
        {
            switch (roleDb.ToUpper())
            {
                case "ADMIN": return "Admin";
                case "USER_MANAGER": return "UserManager";
                default: return "User";
            }
        }
    }

    public class UserDto
    {
        public int UtilizadorId { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Tipo { get; set; } = "User";
        public string? NewPassword { get; set; }
    }
}
