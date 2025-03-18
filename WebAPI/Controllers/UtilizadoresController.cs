using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // Registro de conta aberto a todos – primeiro usuário torna-se ADMIN
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

        [HttpGet("{id}")]
        public async Task<ActionResult<Utilizador>> GetById(int id)
        {
            var user = await _context.Utilizadores.Include(u => u.TipoUtilizador)
                .FirstOrDefaultAsync(u => u.UtilizadorId == id);
            if (user == null) return NotFound();
            return user;
        }

        // Outros métodos (GetAll, Update, Delete) mantidos conforme a versão anterior.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetAll()
        {
            return await _context.Utilizadores.Include(u => u.TipoUtilizador).ToListAsync();
        }

        [HttpPut("{id}")]
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Utilizadores.FindAsync(id);
            if (user == null) return NotFound();

            _context.Utilizadores.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
