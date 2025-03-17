using ES2_TP_ComparadorPrecos_WebAPI.Context;
using ES2_TP_ComparadorPrecos_WebAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2_TP_ComparadorPrecos_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistosPrecosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistosPrecosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistosPreco>>> GetAll()
        {
            return await _context.RegistosPrecos
                .Include(r => r.Produto)
                .Include(r => r.Loja)
                .Include(r => r.Utilizador)
                .Include(r => r.TipoAcao)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegistosPreco>> GetById(int id)
        {
            var registo = await _context.RegistosPrecos
                .Include(r => r.Produto)
                .Include(r => r.Loja)
                .Include(r => r.Utilizador)
                .Include(r => r.TipoAcao)
                .FirstOrDefaultAsync(r => r.RegistoPrecoId == id);

            if (registo == null) return NotFound();
            return registo;
        }

        [HttpPost]
        public async Task<ActionResult<RegistosPreco>> Create(RegistosPreco registo)
        {
            _context.RegistosPrecos.Add(registo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = registo.RegistoPrecoId }, registo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RegistosPreco registo)
        {
            if (id != registo.RegistoPrecoId) return BadRequest();

            _context.Entry(registo).State = EntityState.Modified;
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
            var registo = await _context.RegistosPrecos.FindAsync(id);
            if (registo == null) return NotFound();

            _context.RegistosPrecos.Remove(registo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.RegistosPrecos.Any(e => e.RegistoPrecoId == id);
        }
    }
}
