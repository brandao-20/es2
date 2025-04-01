using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todas as ações requerem autenticação
    public class MensagensController : ControllerBase
    {
        private readonly IMensagemRepository _mensagemRepository;

        public MensagensController(IMensagemRepository mensagemRepository)
        {
            _mensagemRepository = mensagemRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Mensagem>>> GetAll()
        {
            return Ok(await _mensagemRepository.GetAllWithDetailsAsync());
        }

        [HttpGet("my-messages")]
        public async Task<ActionResult<IEnumerable<Mensagem>>> GetMyMessages()
        {
            var userId = int.Parse(User.FindFirst("utilizadorId")?.Value ?? "0");
            return Ok(await _mensagemRepository.GetByUserIdAsync(userId));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Mensagem>> GetById(int id)
        {
            try
            {
                var mensagem = await _mensagemRepository.GetByIdAsync(id);
                return Ok(mensagem);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<Mensagem>> SendMessage([FromBody] Mensagem mensagem)
        {
            var userId = int.Parse(User.FindFirst("utilizadorId")?.Value ?? "0");
            mensagem.RemetenteId = userId;
            mensagem.DataEnvio = DateTime.UtcNow;

            await _mensagemRepository.AddAsync(mensagem);
            return CreatedAtAction(nameof(GetById), new { id = mensagem.MensagemId }, mensagem);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var mensagem = await _mensagemRepository.GetByIdAsync(id);
                await _mensagemRepository.DeleteAsync(mensagem);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
