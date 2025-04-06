using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MensagensController : ControllerBase
    {
        private readonly IMensagemRepository _mensagemRepository;

        public MensagensController(IMensagemRepository mensagemRepository)
        {
            _mensagemRepository = mensagemRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Mensagem>>> GetMessagesByUser(int userId)
        {
            var mensagens = await _mensagemRepository.GetByUserIdAsync(userId);
            return Ok(mensagens);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Mensagem>> GetMessage(int id)
        {
            var mensagem = await _mensagemRepository.GetByIdAsync(id);
            return Ok(mensagem);
        }

        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<List<Mensagem>>> GetAllMessages()
        {
            var mensagens = await _mensagemRepository.GetAllWithDetailsAsync();
            return Ok(mensagens);
        }

        [HttpPost]
        public async Task<ActionResult<Mensagem>> CreateMessage(Mensagem mensagem)
        {
            await _mensagemRepository.AddAsync(mensagem);
            return CreatedAtAction(nameof(GetMessage), new { id = mensagem.MensagemId }, mensagem);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var mensagem = await _mensagemRepository.GetByIdAsync(id);
            await _mensagemRepository.DeleteAsync(mensagem);
            return NoContent();
        }
    }
}
