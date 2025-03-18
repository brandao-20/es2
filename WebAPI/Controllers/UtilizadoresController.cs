using WebAPI.Context;
using WebAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetAll()
        {
            return await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Utilizador>> GetById(int id)
        {
            var user = await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .FirstOrDefaultAsync(u => u.UtilizadorId == id);

            if (user == null) return NotFound();
            return user;
        }

        [HttpPost]
        public async Task<ActionResult<Utilizador>> Create(Utilizador utilizador)
        {
            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = utilizador.UtilizadorId }, utilizador);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Utilizador utilizador)
        {
            if (id != utilizador.UtilizadorId) return BadRequest();
            _context.Entry(utilizador).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(id)) return NotFound();
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

        private bool Exists(int id)
        {
            return _context.Utilizadores.Any(e => e.UtilizadorId == id);
        }
    }
}
