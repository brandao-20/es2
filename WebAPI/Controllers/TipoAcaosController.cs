using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoAcaosController : ControllerBase
    {
        private readonly ITipoAcaoRepository _tipoAcaoRepository;

        public TipoAcaosController(ITipoAcaoRepository tipoAcaoRepository)
        {
            _tipoAcaoRepository = tipoAcaoRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoAcao>>> GetAll()
        {
            return Ok(await _tipoAcaoRepository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoAcao>> GetById(int id)
        {
            var tipo = await _tipoAcaoRepository.GetByIdAsync(id);
            if (tipo == null) return NotFound();
            return tipo;
        }

        [HttpPost]
        public async Task<ActionResult<TipoAcao>> Create(TipoAcao tipo)
        {
            await _tipoAcaoRepository.AddAsync(tipo);
            return CreatedAtAction(nameof(GetById), new { id = tipo.TipoAcaoId }, tipo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TipoAcao tipo)
        {
            if (id != tipo.TipoAcaoId) return BadRequest();

            try
            {
                await _tipoAcaoRepository.UpdateAsync(tipo);
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
            var tipo = await _tipoAcaoRepository.GetByIdAsync(id);
            if (tipo == null) return NotFound();

            await _tipoAcaoRepository.DeleteAsync(tipo);
            return NoContent();
        }
    }
}
