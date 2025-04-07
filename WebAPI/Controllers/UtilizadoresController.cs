using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Entities;
using WebAPI.Helpers;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilizadoresController : ControllerBase
    {
        private readonly IUtilizadorRepository _utilizadorRepository;
        private readonly ITipoUtilizadorRepository _tipoUtilizadorRepository;

        public UtilizadoresController(
            IUtilizadorRepository utilizadorRepository,
            ITipoUtilizadorRepository tipoUtilizadorRepository)
        {
            _utilizadorRepository = utilizadorRepository;
            _tipoUtilizadorRepository = tipoUtilizadorRepository;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Utilizador>> Register(Utilizador utilizador)
        {
            var tiposExistentes = await _utilizadorRepository.GetAllAsync();
            if (!tiposExistentes.Any())
            {
                var adminTipo = (await _tipoUtilizadorRepository.FindAsync(t => t.Tipo.ToLower() == "admin")).FirstOrDefault();
                if (adminTipo == null)
                {
                    adminTipo = new TipoUtilizador { Tipo = "Admin" };
                    await _tipoUtilizadorRepository.AddAsync(adminTipo);
                }
                utilizador.TipoUtilizadorId = adminTipo.TipoUtilizadorId;
            }
            else
            {
                var userTipo = (await _tipoUtilizadorRepository.FindAsync(t => t.Tipo.ToLower() == "user")).FirstOrDefault();
                if (userTipo == null)
                {
                    userTipo = new TipoUtilizador { Tipo = "User" };
                    await _tipoUtilizadorRepository.AddAsync(userTipo);
                }
                utilizador.TipoUtilizadorId = userTipo.TipoUtilizadorId;
            }

            utilizador.Password = PasswordHelper.HashPassword(utilizador.Password);
            await _utilizadorRepository.AddAsync(utilizador);
            return CreatedAtAction(nameof(GetById), new { id = utilizador.UtilizadorId }, utilizador);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Utilizador>> GetById(int id)
        {
            var user = await _utilizadorRepository.GetByIdWithDetailsAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // GET: api/Utilizadores?page=1&pageSize=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var totalItems = await _utilizadorRepository.CountAsync();
            var skip = (page - 1) * pageSize;
            var users = await _utilizadorRepository.GetPagedWithDetailsAsync(skip, pageSize);

            Response.Headers["X-Total-Count"] = totalItems.ToString();
            return Ok(users);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Utilizador utilizador)
        {
            if (id != utilizador.UtilizadorId) return BadRequest();

            var existingUser = await _utilizadorRepository.GetByIdAsync(id);
            if (existingUser != null && utilizador.Password != existingUser.Password)
            {
                utilizador.Password = PasswordHelper.HashPassword(utilizador.Password);
            }

            try
            {
                await _utilizadorRepository.UpdateAsync(utilizador);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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

            var user = await _utilizadorRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            await _utilizadorRepository.DeleteAsync(user);
            return NoContent();
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userIdClaim = User.FindFirst("utilizadorId");
            if (userIdClaim == null) return Unauthorized("Usuário não identificado.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("ID do usuário inválido.");

            var user = await _utilizadorRepository.GetByIdAsync(userId);
            if (user == null) return NotFound();

            await _utilizadorRepository.DeleteAsync(user);
            return NoContent();
        }

        [HttpPost("admincreate")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> AdminCreate(UserDto dto)
        {
            var roleDb = dto.Tipo.ToUpper();
            var tipoEntity = (await _tipoUtilizadorRepository.FindAsync(t => t.Tipo == roleDb)).FirstOrDefault();
            if (tipoEntity == null)
            {
                tipoEntity = new TipoUtilizador { Tipo = roleDb };
                await _tipoUtilizadorRepository.AddAsync(tipoEntity);
            }

            var user = new Utilizador
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = PasswordHelper.HashPassword(dto.Password),
                TipoUtilizadorId = tipoEntity.TipoUtilizadorId,
                DataCriacao = DateTime.UtcNow
            };

            await _utilizadorRepository.AddAsync(user);
            return Ok(user);
        }

        [HttpGet("adminget/{id:int}")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<ActionResult<UserDto>> AdminGetUser(int id)
        {
            var user = await _utilizadorRepository.GetByIdWithDetailsAsync(id);
            if (user == null) return NotFound();

            var roleDb = user.TipoUtilizador?.Tipo ?? "USER";
            var dto = new UserDto
            {
                UtilizadorId = user.UtilizadorId,
                Username = user.Username,
                Email = user.Email,
                Tipo = NormalizeRoleFromDb(roleDb)
            };
            return dto;
        }

        [HttpPut("adminedit/{id:int}")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> AdminEditUser(int id, UserDto dto)
        {
            var user = await _utilizadorRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.Username = dto.Username;
            user.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                user.Password = PasswordHelper.HashPassword(dto.NewPassword);
            }

            var roleDb = dto.Tipo.ToUpper();
            var tipoEntity = (await _tipoUtilizadorRepository.FindAsync(t => t.Tipo == roleDb)).FirstOrDefault();
            if (tipoEntity == null)
            {
                tipoEntity = new TipoUtilizador { Tipo = roleDb };
                await _tipoUtilizadorRepository.AddAsync(tipoEntity);
            }
            user.TipoUtilizadorId = tipoEntity.TipoUtilizadorId;

            await _utilizadorRepository.UpdateAsync(user);
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
