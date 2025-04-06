using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistosPrecosController : ControllerBase
    {
        private readonly IRegistosPrecoRepository _registosPrecoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly ILojaRepository _lojaRepository;

        public RegistosPrecosController(
            IRegistosPrecoRepository registosPrecoRepository,
            IProdutoRepository produtoRepository,
            ILojaRepository lojaRepository)
        {
            _registosPrecoRepository = registosPrecoRepository;
            _produtoRepository = produtoRepository;
            _lojaRepository = lojaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistosPreco>>> GetAll()
        {
            return Ok(await _registosPrecoRepository.GetAllWithDetailsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegistosPreco>> GetById(int id)
        {
            var registo = await _registosPrecoRepository.GetByIdWithDetailsAsync(id);
            if (registo == null) return NotFound();
            return registo;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RegistosPreco>> Create(RegistosPreco registo)
        {
            try
            {
                bool prodExists = await _produtoRepository.ExistsAsync(registo.ProdutoId);
                if (!prodExists) return BadRequest(new { message = $"Produto {registo.ProdutoId} não existe." });

                bool storeExists = await _lojaRepository.ExistsAsync(registo.LojaId);
                if (!storeExists) return BadRequest(new { message = $"Loja {registo.LojaId} não existe." });

                if (registo.DataRegisto == default)
                    registo.DataRegisto = DateTime.UtcNow;

                registo.Credibilidade = 1; // Valor inicial
                registo.UtilizadorId = int.Parse(User.FindFirst("utilizadorId")?.Value ?? "0");

                await _registosPrecoRepository.AddAsync(registo);
                return CreatedAtAction(nameof(GetById), new { id = registo.RegistoPrecoId }, registo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, "Ocorreu um erro interno: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, RegistosPreco registo)
        {
            if (id != registo.RegistoPrecoId) return BadRequest();

            try
            {
                await _registosPrecoRepository.UpdateAsync(registo);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var registo = await _registosPrecoRepository.GetByIdAsync(id);
            if (registo == null) return NotFound();

            await _registosPrecoRepository.DeleteAsync(registo);
            return NoContent();
        }

        [HttpPost("confirm/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmPrice(int id)
        {
            var registo = await _registosPrecoRepository.GetByIdAsync(id);
            if (registo == null) return NotFound();

            registo.Credibilidade = Math.Min(registo.Credibilidade + 10, 100);
            registo.DataRegisto = DateTime.UtcNow; // Atualiza a data para refletir a confirmação
            await _registosPrecoRepository.UpdateAsync(registo);

            return Ok(new { message = "Preço confirmado com sucesso", credibilidade = registo.Credibilidade });
        }
    }
}
