using ES2_TP_ComparadorPrecos_WebAPI.Context;
using ES2_TP_ComparadorPrecos_WebAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2_TP_ComparadorPrecos_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoAcaosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoAcaosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoAcao>>> GetAll()
        {
            return await _context.TipoAcaos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoAcao>> GetById(int id)
        {
            var tipo = await _context.TipoAcaos.FindAsync(id);
            if (tipo == null) return NotFound();
            return tipo;
        }

        [HttpPost]
        public async Task<ActionResult<TipoAcao>> Create(TipoAcao tipo)
        {
            _context.TipoAcaos.Add(tipo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tipo.TipoAcaoId }, tipo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TipoAcao tipo)
        {
            if (id != tipo.TipoAcaoId) return BadRequest();
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
            var tipo = await _context.TipoAcaos.FindAsync(id);
            if (tipo == null) return NotFound();

            _context.TipoAcaos.Remove(tipo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.TipoAcaos.Any(e => e.TipoAcaoId == id);
        }
    }
}
