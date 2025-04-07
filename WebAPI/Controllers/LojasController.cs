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

        // GET: api/Lojas?page=1&pageSize=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loja>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var totalItems = await _lojaRepository.CountAsync();
            var skip = (page - 1) * pageSize;
            var lojas = await _lojaRepository.GetPagedWithDetailsAsync(skip, pageSize);

            Response.Headers["X-Total-Count"] = totalItems.ToString();
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
                if (string.IsNullOrWhiteSpace(loja.Localizacao.GoogleMapsUrl) &&
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
                if (string.IsNullOrWhiteSpace(loja.Localizacao.GoogleMapsUrl) &&
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
    }
}
