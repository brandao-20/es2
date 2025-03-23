using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Controllers
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
            // Carrega as lojas com a respectiva Localizacao
            var lojas = await _context.Lojas
                .Include(l => l.Localizacao)
                .ToListAsync();
            return lojas;
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
            // Se vier uma Localizacao com lat/long mas sem GoogleMapsUrl, geramos
            if (loja.Localizacao != null)
            {
                if (!HasValue(loja.Localizacao.GoogleMapsUrl) &&
                    loja.Localizacao.Latitude.HasValue &&
                    loja.Localizacao.Longitude.HasValue)
                {
                    loja.Localizacao.GoogleMapsUrl =
                        $"https://maps.google.com/?q={loja.Localizacao.Latitude},{loja.Localizacao.Longitude}";
                }
            }

            _context.Lojas.Add(loja);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = loja.LojaId }, loja);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Loja loja)
        {
            if (id != loja.LojaId) return BadRequest();

            // Rastreia a entidade
            _context.Entry(loja).State = EntityState.Modified;

            // Se vier Localizacao, também rastreia
            if (loja.Localizacao != null)
            {
                if (loja.LocalizacaoId == null || loja.LocalizacaoId == 0)
                {
                    // Nova localizacao
                    _context.Entry(loja.Localizacao).State = EntityState.Added;
                }
                else
                {
                    _context.Entry(loja.Localizacao).State = EntityState.Modified;
                }

                // Se não tiver URL, gera
                if (!HasValue(loja.Localizacao.GoogleMapsUrl) &&
                    loja.Localizacao.Latitude.HasValue &&
                    loja.Localizacao.Longitude.HasValue)
                {
                    loja.Localizacao.GoogleMapsUrl =
                        $"https://maps.google.com/?q={loja.Localizacao.Latitude},{loja.Localizacao.Longitude}";
                }
            }

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

        private bool HasValue(string? str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }
    }
}
