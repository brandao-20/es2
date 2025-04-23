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
            try
            {
                var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();
                return Ok(registos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao buscar todos os registros de preços: {ex.Message}\n{ex.StackTrace}");
                return BadRequest(new { message = "Erro ao buscar os registros de preços.", detail = ex.Message });
            }
        }

        [HttpGet("produto/{produtoId}")]
        public async Task<ActionResult<IEnumerable<RegistosPreco>>> GetByProdutoId(int produtoId)
        {
            try
            {
                var registos = await _registosPrecoRepository.GetByProdutoIdAsync(produtoId);
                if (registos == null || !registos.Any())
                    return NotFound(new { message = "Nenhum registro de preço encontrado para este produto." });
                return Ok(registos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao buscar registros de preços para o produto {produtoId}: {ex.Message}\n{ex.StackTrace}");
                return BadRequest(new { message = "Erro ao buscar os registros de preços.", detail = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegistosPreco>> GetById(int id)
        {
            var registo = await _registosPrecoRepository.GetByIdWithDetailsAsync(id);
            if (registo == null) 
                return NotFound();
            return Ok(registo);
        }

        [HttpGet("{id}/credibility")]
        public async Task<ActionResult<decimal>> GetAdjustedCredibility(int id)
        {
            var registo = await _registosPrecoRepository.GetByIdWithDetailsAsync(id);
            if (registo == null) 
                return NotFound();

            var meses = (DateTime.UtcNow - registo.DataRegisto).TotalDays / 30;
            var adjusted = registo.Credibilidade - (decimal)(0.1 * meses);
            if (adjusted < 0)
                adjusted = 0;
            return Ok(adjusted);
        }

        [HttpGet("latest/{produtoId:int}/{lojaId:int}")]
        public async Task<ActionResult<RegistosPreco>> GetLatestPrice(int produtoId, int lojaId)
        {
            var latest = await _registosPrecoRepository.GetLatestPriceAsync(produtoId, lojaId);
            if (latest == null)
                return NotFound(new { message = "Nenhum preço registado para este produto nesta loja." });
            return Ok(latest);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RegistosPreco>> Create(RegistosPreco registo)
        {
            try
            {
                if (registo.Preco <= 0)
                    return BadRequest(new { message = "O preço deve ser maior que zero." });

                bool prodExists = await _produtoRepository.ExistsAsync(registo.ProdutoId);
                if (!prodExists)
                    return BadRequest(new { message = $"Produto {registo.ProdutoId} não existe." });

                bool storeExists = await _lojaRepository.ExistsAsync(registo.LojaId);
                if (!storeExists)
                    return BadRequest(new { message = $"Loja {registo.LojaId} não existe." });

                if (registo.DataRegisto > DateTime.UtcNow)
                    return BadRequest(new { message = "Data de registo não pode ser futura." });

                if (registo.DataRegisto == default)
                    registo.DataRegisto = DateTime.UtcNow;

                registo.Produto = null;
                registo.Loja = null;
                registo.TipoAcao = null;

                registo.Credibilidade = 1;
                registo.UtilizadorId = int.Parse(User.FindFirst("utilizadorId")?.Value ?? "0");

                await _registosPrecoRepository.AddAsync(registo);
                return CreatedAtAction(nameof(GetById), new { id = registo.RegistoPrecoId }, registo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = "Ocorreu um erro interno.", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, RegistosPreco registo)
        {
            if (id != registo.RegistoPrecoId)
                return BadRequest("ID do registo não coincide.");

            if (registo.DataRegisto > DateTime.UtcNow)
                return BadRequest(new { message = "Data de registo não pode ser futura." });

            try
            {
                await _registosPrecoRepository.UpdateAsync(registo);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Registo de preço não encontrado.");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var registo = await _registosPrecoRepository.GetByIdAsync(id);
            if (registo == null)
                return NotFound("Registo de preço não encontrado.");

            await _registosPrecoRepository.DeleteAsync(registo);
            return NoContent();
        }

        [HttpPost("confirm/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmPrice(int id)
        {
            var registo = await _registosPrecoRepository.GetByIdAsync(id);
            if (registo == null)
                return NotFound("Registo de preço não encontrado.");

            registo.Credibilidade = Math.Min(registo.Credibilidade + 10, 100);
            registo.DataRegisto = DateTime.UtcNow;
            await _registosPrecoRepository.UpdateAsync(registo);

            return Ok(new { message = "Preço confirmado com sucesso", credibilidade = registo.Credibilidade });
        }
    }
}
