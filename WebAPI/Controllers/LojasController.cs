using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LojasController : ControllerBase
    {
        private readonly ILojaRepository _lojaRepository;

        public LojasController(ILojaRepository lojaRepository)
        {
            _lojaRepository = lojaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loja>>> GetAll()
        {
            var lojas = await _lojaRepository.GetAllWithDetailsAsync();
            return Ok(lojas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Loja>> GetById(int id)
        {
            var loja = await _lojaRepository.GetByIdWithDetailsAsync(id);
            if (loja == null) return NotFound();
            return loja;
        }

        [HttpPost]
        public async Task<ActionResult<Loja>> Create(Loja loja)
        {
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

            await _lojaRepository.AddAsync(loja);
            return CreatedAtAction(nameof(GetById), new { id = loja.LojaId }, loja);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Loja loja)
        {
            if (id != loja.LojaId) return BadRequest();

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

            try
            {
                await _lojaRepository.UpdateAsync(loja);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var loja = await _lojaRepository.GetByIdAsync(id);
            if (loja == null) return NotFound();

            await _lojaRepository.DeleteAsync(loja);
            return NoContent();
        }

        private bool HasValue(string? str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }
    }
}
