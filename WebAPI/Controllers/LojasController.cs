using ES2_TP_ComparadorPrecos_WebAPI.Context;
using ES2_TP_ComparadorPrecos_WebAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2_TP_ComparadorPrecos_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LojasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LojasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loja>>> GetAll()
        {
            return await _context.Lojas
                .Include(l => l.Localizacao) // Se quiseres ver dados de Localizacao
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Loja>> GetById(int id)
        {
            var loja = await _context.Lojas
                .Include(l => l.Localizacao)
                .FirstOrDefaultAsync(l => l.LojaId == id);
            if (loja == null) return NotFound();
            return loja;
        }

        [HttpPost]
        public async Task<ActionResult<Loja>> Create(Loja loja)
        {
            _context.Lojas.Add(loja);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = loja.LojaId }, loja);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Loja loja)
        {
            if (id != loja.LojaId) return BadRequest();

            _context.Entry(loja).State = EntityState.Modified;
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
            var loja = await _context.Lojas.FindAsync(id);
            if (loja == null) return NotFound();

            _context.Lojas.Remove(loja);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.Lojas.Any(e => e.LojaId == id);
        }
    }
}
