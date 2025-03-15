using ES2_TP_ComparadorPrecos_WebAPI.Context;
using ES2_TP_ComparadorPrecos_WebAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2_TP_ComparadorPrecos_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalizacoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocalizacoesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Localizacao>>> GetAll()
        {
            return await _context.Localizacaos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Localizacao>> GetById(int id)
        {
            var localizacao = await _context.Localizacaos.FindAsync(id);
            if (localizacao == null) return NotFound();
            return localizacao;
        }

        [HttpPost]
        public async Task<ActionResult<Localizacao>> Create(Localizacao localizacao)
        {
            _context.Localizacaos.Add(localizacao);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = localizacao.LocalizacaoId }, localizacao);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Localizacao localizacao)
        {
            if (id != localizacao.LocalizacaoId) return BadRequest();

            _context.Entry(localizacao).State = EntityState.Modified;
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
            var localizacao = await _context.Localizacaos.FindAsync(id);
            if (localizacao == null) return NotFound();

            _context.Localizacaos.Remove(localizacao);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.Localizacaos.Any(e => e.LocalizacaoId == id);
        }
    }
}
