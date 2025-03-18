using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoUtilizadoresController : ControllerBase
    {
        private readonly AppDbContext _context;
    
        public TipoUtilizadoresController(AppDbContext context)
        {
            _context = context;
        }
    
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoUtilizador>>> GetAll()
        {
            return await _context.TipoUtilizadors.ToListAsync();
        }
    
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoUtilizador>> GetById(int id)
        {
            var tipo = await _context.TipoUtilizadors.FindAsync(id);
            if (tipo == null) return NotFound();
            return tipo;
        }
    
        [HttpPost]
        public async Task<ActionResult<TipoUtilizador>> Create(TipoUtilizador tipo)
        {
            _context.TipoUtilizadors.Add(tipo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tipo.TipoUtilizadorId }, tipo);
        }
    
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TipoUtilizador tipo)
        {
            if (id != tipo.TipoUtilizadorId) return BadRequest();
            _context.Entry(tipo).State = EntityState.Modified;
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
            var tipo = await _context.TipoUtilizadors.FindAsync(id);
            if (tipo == null) return NotFound();
    
            _context.TipoUtilizadors.Remove(tipo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    
        private bool Exists(int id)
        {
            return _context.TipoUtilizadors.Any(e => e.TipoUtilizadorId == id);
        }
    }
}
