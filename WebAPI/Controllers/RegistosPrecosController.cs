using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
using WebAPI.Repositories;
using WebAPI.Helpers;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistosPrecosController : ControllerBase
    {
        private readonly IRegistosPrecoRepository _registosPrecoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly ILojaRepository _lojaRepository;
        private readonly ILogger<RegistosPrecosController> _logger;

        public RegistosPrecosController(
            IRegistosPrecoRepository registosPrecoRepository,
            IProdutoRepository produtoRepository,
            ILojaRepository lojaRepository,
            ILogger<RegistosPrecosController> logger)
        {
            _registosPrecoRepository = registosPrecoRepository;
            _produtoRepository = produtoRepository;
            _lojaRepository = lojaRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<RegistoPrecoModel>>>> GetAll()
        {
            _logger.LogInformation("[DEBUG] Obtendo todos os registros de preços.");

            try
            {
                var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();
                var modelos = registos.Select(r => new RegistoPrecoModel
                {
                    RegistoId = r.RegistoPrecoId,
                    ProdutoId = r.ProdutoId,
                    ProdutoNome = r.Produto?.Nome ?? "",
                    LojaId = r.LojaId,
                    LojaNome = r.Loja?.Nome ?? "",
                    Preco = r.Preco,
                    DataRegisto = r.DataRegisto
                }).ToList();

                _logger.LogInformation($"[DEBUG] Registros de preços obtidos: {modelos.Count}");
                return Ok(ApiResponse<IEnumerable<RegistoPrecoModel>>.SuccessResponse(modelos, "Registros de preços obtidos com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao buscar todos os registros de preços: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, ApiResponse<IEnumerable<RegistoPrecoModel>>.ErrorResponse("Erro ao buscar os registros de preços.", "SERVER_ERROR", 500));
            }
        }

        [HttpGet("produto/{produtoId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RegistosPreco>>>> GetByProdutoId(int produtoId)
        {
            if (produtoId <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {produtoId}");
                return BadRequest(ApiResponse<IEnumerable<RegistosPreco>>.ErrorResponse("ID do produto deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Obtendo registros de preços para o produto com ID: {produtoId}");

            try
            {
                var registos = await _registosPrecoRepository.GetByProdutoIdAsync(produtoId);
                if (registos == null || !registos.Any())
                {
                    _logger.LogWarning($"[DEBUG] Nenhum registro de preço encontrado para o produto com ID: {produtoId}");
                    return NotFound(ApiResponse<IEnumerable<RegistosPreco>>.ErrorResponse("Nenhum registro de preço encontrado para este produto.", "NOT_FOUND", 404));
                }

                return Ok(ApiResponse<IEnumerable<RegistosPreco>>.SuccessResponse(registos, "Registros de preços obtidos com sucesso."));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao buscar registros de preços para o produto {produtoId}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, ApiResponse<IEnumerable<RegistosPreco>>.ErrorResponse("Erro ao buscar os registros de preços.", "SERVER_ERROR", 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RegistosPreco>>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<RegistosPreco>.ErrorResponse("ID do registro deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Obtendo registro de preço com ID: {id}");

            var registo = await _registosPrecoRepository.GetByIdWithDetailsAsync(id);
            if (registo == null)
            {
                _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado.");
                return NotFound(ApiResponse<RegistosPreco>.ErrorResponse("Registro de preço não encontrado.", "NOT_FOUND", 404));
            }

            return Ok(ApiResponse<RegistosPreco>.SuccessResponse(registo, "Registro de preço obtido com sucesso."));
        }

        [HttpGet("{id}/credibility")]
        public async Task<ActionResult<ApiResponse<object>>> GetAdjustedCredibility(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do registro deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Calculando credibilidade ajustada para o registro com ID: {id}");

            var registo = await _registosPrecoRepository.GetByIdWithDetailsAsync(id);
            if (registo == null)
            {
                _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado.");
                return NotFound(ApiResponse<object>.ErrorResponse("Registro de preço não encontrado.", "NOT_FOUND", 404));
            }

            var meses = (DateTime.UtcNow - registo.DataRegisto).TotalDays / 30;
            var adjusted = registo.Credibilidade - (decimal)(0.1 * meses);
            if (adjusted < 0) adjusted = 0;

            _logger.LogInformation($"[DEBUG] Credibilidade ajustada para o registro {id}: {adjusted}");
            return Ok(ApiResponse<object>.SuccessResponse(new { Credibilidade = adjusted }, "Credibilidade ajustada calculada com sucesso."));
        }

        [HttpGet("latest/{produtoId:int}/{lojaId:int}")]
        public async Task<ActionResult<ApiResponse<RegistosPreco>>> GetLatestPrice(int produtoId, int lojaId)
        {
            if (produtoId <= 0 || lojaId <= 0)
            {
                _logger.LogWarning($"[DEBUG] IDs inválidos fornecidos - ProdutoId: {produtoId}, LojaId: {lojaId}");
                return BadRequest(ApiResponse<RegistosPreco>.ErrorResponse("IDs devem ser maiores que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Obtendo último preço para ProdutoId: {produtoId}, LojaId: {lojaId}");

            var latest = await _registosPrecoRepository.GetLatestPriceAsync(produtoId, lojaId);
            if (latest == null)
            {
                _logger.LogInformation($"[DEBUG] Nenhum preço registrado para ProdutoId: {produtoId}, LojaId: {lojaId}");
                return NotFound(ApiResponse<RegistosPreco>.ErrorResponse("Nenhum preço registrado para este produto nesta loja.", "NOT_FOUND", 404));
            }

            return Ok(ApiResponse<RegistosPreco>.SuccessResponse(latest, "Último preço obtido com sucesso."));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RegistosPreco>>> Create(RegistosPreco registo)
        {
            if (registo.ProdutoId <= 0 || registo.LojaId <= 0)
            {
                _logger.LogWarning($"[DEBUG] IDs inválidos fornecidos - ProdutoId: {registo.ProdutoId}, LojaId: {registo.LojaId}");
                return BadRequest(ApiResponse<RegistosPreco>.ErrorResponse("IDs devem ser maiores que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Criando novo registro de preço: ProdutoId={registo.ProdutoId}, LojaId={registo.LojaId}, Preço={registo.Preco}");

            if (registo.Preco <= 0)
            {
                _logger.LogWarning("[DEBUG] Preço inválido ao criar registro de preço.");
                return BadRequest(ApiResponse<RegistosPreco>.ErrorResponse("O preço deve ser maior que zero.", "INVALID_DATA", 400));
            }

            bool prodExists = await _produtoRepository.ExistsAsync(registo.ProdutoId);
            if (!prodExists)
            {
                _logger.LogWarning($"[DEBUG] Produto com ID {registo.ProdutoId} não existe.");
                return BadRequest(ApiResponse<RegistosPreco>.ErrorResponse($"Produto {registo.ProdutoId} não existe.", "INVALID_PRODUCT", 400));
            }

            bool storeExists = await _lojaRepository.ExistsAsync(registo.LojaId);
            if (!storeExists)
            {
                _logger.LogWarning($"[DEBUG] Loja com ID {registo.LojaId} não existe.");
                return BadRequest(ApiResponse<RegistosPreco>.ErrorResponse($"Loja {registo.LojaId} não existe.", "INVALID_STORE", 400));
            }

            if (registo.DataRegisto > DateTime.UtcNow)
            {
                _logger.LogWarning("[DEBUG] Data de registro futura ao criar registro de preço.");
                return BadRequest(ApiResponse<RegistosPreco>.ErrorResponse("Data de registro não pode ser futura.", "INVALID_DATE", 400));
            }

            if (registo.DataRegisto == default)
                registo.DataRegisto = DateTime.UtcNow;

            registo.Produto = null;
            registo.Loja = null;
            registo.TipoAcao = null;

            registo.Credibilidade = 1;
            registo.UtilizadorId = int.Parse(User.FindFirst("utilizadorId")?.Value ?? "0");

            await _registosPrecoRepository.AddAsync(registo);
            _logger.LogInformation($"[DEBUG] Registro de preço criado com sucesso: ID={registo.RegistoPrecoId}");
            return CreatedAtAction(
                nameof(GetById),
                new { id = registo.RegistoPrecoId },
                ApiResponse<RegistosPreco>.SuccessResponse(registo, "Registro de preço criado com sucesso.")
            );
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, RegistosPreco registo)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do registro deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Atualizando registro de preço com ID: {id}, Preço={registo.Preco}");

            if (id != registo.RegistoPrecoId)
            {
                _logger.LogWarning($"[DEBUG] ID do registro ({registo.RegistoPrecoId}) não coincide com o ID da URL ({id}).");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do registro não coincide.", "INVALID_ID", 400));
            }

            if (registo.DataRegisto > DateTime.UtcNow)
            {
                _logger.LogWarning("[DEBUG] Data de registro futura ao atualizar registro de preço.");
                return BadRequest(ApiResponse<object>.ErrorResponse("Data de registro não pode ser futura.", "INVALID_DATE", 400));
            }

            try
            {
                await _registosPrecoRepository.UpdateAsync(registo);
                _logger.LogInformation($"[DEBUG] Registro de preço com ID {id} atualizado com sucesso.");
                return Ok(ApiResponse<object>.SuccessResponse(null, "Registro de preço atualizado com sucesso."));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado para atualização.");
                return NotFound(ApiResponse<object>.ErrorResponse("Registro de preço não encontrado.", "NOT_FOUND", 404));
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do registro deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Deletando registro de preço com ID: {id}");

            var registo = await _registosPrecoRepository.GetByIdAsync(id);
            if (registo == null)
            {
                _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado para deleção.");
                return NotFound(ApiResponse<object>.ErrorResponse("Registro de preço não encontrado.", "NOT_FOUND", 404));
            }

            await _registosPrecoRepository.DeleteAsync(registo);
            _logger.LogInformation($"[DEBUG] Registro de preço com ID {id} deletado com sucesso.");
            return Ok(ApiResponse<object>.SuccessResponse(null, "Registro de preço removido com sucesso."));
        }

        [HttpPost("confirm/{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ConfirmPrice(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do registro deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Confirmando preço para o registro com ID: {id}");

            var registo = await _registosPrecoRepository.GetByIdAsync(id);
            if (registo == null)
            {
                _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado para confirmação.");
                return NotFound(ApiResponse<object>.ErrorResponse("Registro de preço não encontrado.", "NOT_FOUND", 404));
            }

            registo.Credibilidade = Math.Min(registo.Credibilidade + 10, 100);
            registo.DataRegisto = DateTime.UtcNow;
            await _registosPrecoRepository.UpdateAsync(registo);

            _logger.LogInformation($"[DEBUG] Preço confirmado para o registro {id}, nova credibilidade: {registo.Credibilidade}");
            return Ok(ApiResponse<object>.SuccessResponse(
                new { Credibilidade = registo.Credibilidade },
                "Preço confirmado com sucesso."
            ));
        }
    }

    public class RegistoPrecoModel
    {
        public int RegistoId { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = "";
        public int LojaId { get; set; }
        public string LojaNome { get; set; } = "";
        public decimal Preco { get; set; }
        public DateTime DataRegisto { get; set; }
    }
}
