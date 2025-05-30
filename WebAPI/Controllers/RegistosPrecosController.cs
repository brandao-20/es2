using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Entities;
using WebAPI.Hubs;
using WebAPI.Repositories;
using WebAPI.Helpers;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistosPrecosController : ControllerBase
    {
        private readonly IRegistosPrecoRepository _registosPrecoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly ILojaRepository _lojaRepository;
        private readonly IUtilizadorRepository _utilizadorRepository;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<RegistosPrecosController> _logger;

        public RegistosPrecosController(
            IRegistosPrecoRepository registosPrecoRepository,
            IProdutoRepository produtoRepository,
            ILojaRepository lojaRepository,
            IUtilizadorRepository utilizadorRepository,
            IHubContext<ChatHub> hubContext,
            ILogger<RegistosPrecosController> logger)
        {
            _registosPrecoRepository = registosPrecoRepository;
            _produtoRepository = produtoRepository;
            _lojaRepository = lojaRepository;
            _utilizadorRepository = utilizadorRepository;
            _hubContext = hubContext;
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
                return Ok(new ApiResponse<IEnumerable<RegistoPrecoModel>>
                {
                    Success = true,
                    Message = "Registros de preços obtidos com sucesso.",
                    StatusCode = 200,
                    Data = modelos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao buscar todos os registros de preços: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<IEnumerable<RegistoPrecoModel>>
                {
                    Success = false,
                    Message = "Erro ao buscar os registros de preços.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpGet("produto/{produtoId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RegistosPreco>>>> GetByProdutoId(int produtoId)
        {
            if (produtoId <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {produtoId}");
                return BadRequest(new ApiResponse<IEnumerable<RegistosPreco>>
                {
                    Success = false,
                    Message = "ID do produto deve ser maior que zero.",
                    ErrorCode = "INVALID_ID",
                    StatusCode = 400,
                    Data = null
                });
            }

            _logger.LogInformation($"[DEBUG] Obtendo registros de preços para o produto com ID: {produtoId}");
            try
            {
                var registos = await _registosPrecoRepository.GetByProdutoIdAsync(produtoId);
                if (registos == null || !registos.Any())
                {
                    _logger.LogWarning($"[DEBUG] Nenhum registro de preço encontrado para o produto com ID: {produtoId}");
                    return NotFound(new ApiResponse<IEnumerable<RegistosPreco>>
                    {
                        Success = false,
                        Message = "Nenhum registro de preço encontrado para este produto.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                _logger.LogInformation($"[DEBUG] Registros de preços encontrados: {registos.Count()}");
                return Ok(new ApiResponse<IEnumerable<RegistosPreco>>
                {
                    Success = true,
                    Message = "Registros de preços obtidos com sucesso.",
                    StatusCode = 200,
                    Data = registos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao buscar registros de preços para o produto {produtoId}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<IEnumerable<RegistosPreco>>
                {
                    Success = false,
                    Message = "Erro ao buscar os registros de preços.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RegistosPreco>>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(new ApiResponse<RegistosPreco>
                {
                    Success = false,
                    Message = "ID do registro deve ser maior que zero.",
                    ErrorCode = "INVALID_ID",
                    StatusCode = 400,
                    Data = null
                });
            }

            _logger.LogInformation($"[DEBUG] Obtendo registro de preço com ID: {id}");
            try
            {
                var registo = await _registosPrecoRepository.GetByIdWithDetailsAsync(id);
                if (registo == null)
                {
                    _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado.");
                    return NotFound(new ApiResponse<RegistosPreco>
                    {
                        Success = false,
                        Message = "Registro de preço não encontrado.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                _logger.LogInformation($"[DEBUG] Registro de preço obtido: ID={registo.RegistoPrecoId}");
                return Ok(new ApiResponse<RegistosPreco>
                {
                    Success = true,
                    Message = "Registro de preço obtido com sucesso.",
                    StatusCode = 200,
                    Data = registo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao buscar registro de preço com ID {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<RegistosPreco>
                {
                    Success = false,
                    Message = "Erro ao buscar o registro de preço.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpGet("{id}/credibility")]
        public async Task<ActionResult<ApiResponse<object>>> GetAdjustedCredibility(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID do registro deve ser maior que zero.",
                    ErrorCode = "INVALID_ID",
                    StatusCode = 400,
                    Data = null
                });
            }

            _logger.LogInformation($"[DEBUG] Calculando credibilidade ajustada para o registro com ID: {id}");
            try
            {
                var registo = await _registosPrecoRepository.GetByIdWithDetailsAsync(id);
                if (registo == null)
                {
                    _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado.");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Registro de preço não encontrado.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                var meses = (DateTime.UtcNow - registo.DataRegisto).TotalDays / 30;
                var adjusted = registo.Credibilidade - (decimal)(0.1 * meses);
                if (adjusted < 0) adjusted = 0;

                _logger.LogInformation($"[DEBUG] Credibilidade ajustada para o registro {id}: {adjusted}");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Credibilidade ajustada calculada com sucesso.",
                    StatusCode = 200,
                    Data = new { Credibilidade = adjusted }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao calcular credibilidade para o registro {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erro ao calcular a credibilidade.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpGet("latest/{produtoId:int}/{lojaId:int}")]
        public async Task<ActionResult<ApiResponse<RegistosPreco>>> GetLatestPrice(int produtoId, int lojaId)
        {
            if (produtoId <= 0 || lojaId <= 0)
            {
                _logger.LogWarning($"[DEBUG] IDs inválidos fornecidos - ProdutoId: {produtoId}, LojaId: {lojaId}");
                return BadRequest(new ApiResponse<RegistosPreco>
                {
                    Success = false,
                    Message = "IDs devem ser maiores que zero.",
                    ErrorCode = "INVALID_ID",
                    StatusCode = 400,
                    Data = null
                });
            }

            _logger.LogInformation($"[DEBUG] Obtendo último preço para ProdutoId: {produtoId}, LojaId: {lojaId}");
            try
            {
                var latest = await _registosPrecoRepository.GetLatestPriceAsync(produtoId, lojaId);
                if (latest == null)
                {
                    _logger.LogInformation($"[DEBUG] Nenhum preço registrado para ProdutoId: {produtoId}, LojaId: {lojaId}");
                    return NotFound(new ApiResponse<RegistosPreco>
                    {
                        Success = false,
                        Message = "Nenhum preço registrado para este produto nesta loja.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                _logger.LogInformation($"[DEBUG] Último preço obtido: {latest.Preco}");
                return Ok(new ApiResponse<RegistosPreco>
                {
                    Success = true,
                    Message = "Último preço obtido com sucesso.",
                    StatusCode = 200,
                    Data = latest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao buscar último preço para ProdutoId {produtoId}, LojaId {lojaId}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<RegistosPreco>
                {
                    Success = false,
                    Message = "Erro ao buscar o último preço.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RegistosPreco>>> Create(RegistosPreco registo)
        {
            if (registo.ProdutoId <= 0 || registo.LojaId <= 0)
            {
                _logger.LogWarning($"[DEBUG] IDs inválidos fornecidos - ProdutoId: {registo.ProdutoId}, LojaId: {registo.LojaId}");
                return BadRequest(new ApiResponse<RegistosPreco>
                {
                    Success = false,
                    Message = "IDs devem ser maiores que zero.",
                    ErrorCode = "INVALID_ID",
                    StatusCode = 400,
                    Data = null
                });
            }

            _logger.LogInformation($"[DEBUG] Criando novo registro de preço: ProdutoId={registo.ProdutoId}, LojaId={registo.LojaId}, Preço={registo.Preco}");
            try
            {
                if (registo.Preco <= 0)
                {
                    _logger.LogWarning("[DEBUG] Preço inválido ao criar registro de preço.");
                    return BadRequest(new ApiResponse<RegistosPreco>
                    {
                        Success = false,
                        Message = "O preço deve ser maior que zero.",
                        ErrorCode = "INVALID_DATA",
                        StatusCode = 400,
                        Data = null
                    });
                }

                bool prodExists = await _produtoRepository.ExistsAsync(registo.ProdutoId);
                if (!prodExists)
                {
                    _logger.LogWarning($"[DEBUG] Produto com ID {registo.ProdutoId} não existe.");
                    return BadRequest(new ApiResponse<RegistosPreco>
                    {
                        Success = false,
                        Message = $"Produto {registo.ProdutoId} não existe.",
                        ErrorCode = "INVALID_PRODUCT",
                        StatusCode = 400,
                        Data = null
                    });
                }

                bool storeExists = await _lojaRepository.ExistsAsync(registo.LojaId);
                if (!storeExists)
                {
                    _logger.LogWarning($"[DEBUG] Loja com ID {registo.LojaId} não existe.");
                    return BadRequest(new ApiResponse<RegistosPreco>
                    {
                        Success = false,
                        Message = $"Loja {registo.LojaId} não existe.",
                        ErrorCode = "INVALID_STORE",
                        StatusCode = 400,
                        Data = null
                    });
                }

                if (registo.DataRegisto > DateTime.UtcNow)
                {
                    _logger.LogWarning("[DEBUG] Data de registro futura ao criar registro de preço.");
                    return BadRequest(new ApiResponse<RegistosPreco>
                    {
                        Success = false,
                        Message = "Data de registro não pode ser futura.",
                        ErrorCode = "INVALID_DATE",
                        StatusCode = 400,
                        Data = null
                    });
                }

                if (registo.DataRegisto == default)
                    registo.DataRegisto = DateTime.UtcNow;

                registo.Produto = null;
                registo.Loja = null;
                registo.TipoAcao = null;

                registo.Credibilidade = 1;
                var userIdClaim = User.FindFirst("utilizadorId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("[DEBUG] Usuário não identificado ao criar registro de preço.");
                    return Unauthorized(new ApiResponse<RegistosPreco>
                    {
                        Success = false,
                        Message = "Usuário não identificado.",
                        ErrorCode = "UNAUTHORIZED",
                        StatusCode = 401,
                        Data = null
                    });
                }
                registo.UtilizadorId = userId;

                _logger.LogInformation("[DEBUG] Salvando novo registro no repositório...");
                await _registosPrecoRepository.AddAsync(registo);
                _logger.LogInformation("[DEBUG] Novo registro salvo com sucesso.");

                // Incrementar pontos do utilizador
                _logger.LogInformation($"[DEBUG] Incrementando pontos do utilizador {userId}...");
                var utilizador = await _utilizadorRepository.GetByIdAsync(userId);
                if (utilizador != null)
                {
                    utilizador.Pontos += 5; // +5 pontos por registo
                    _logger.LogInformation($"[DEBUG] Novos pontos do utilizador {userId}: {utilizador.Pontos}");
                    await _utilizadorRepository.UpdateAsync(utilizador);
                    _logger.LogInformation($"[DEBUG] Pontos do utilizador {userId} atualizados com sucesso.");
                }
                else
                {
                    _logger.LogWarning($"[DEBUG] Utilizador {userId} não encontrado para incrementar pontos.");
                }

                // Notificar utilizadores que favoritaram o produto
                _logger.LogInformation("[DEBUG] Enviando notificação de mudança de preço...");
                await _hubContext.Clients.All.SendAsync("PriceChanged", registo.ProdutoId, registo.Preco);
                _logger.LogInformation($"[DEBUG] Notificação de preço enviada para ProdutoId: {registo.ProdutoId}");

                _logger.LogInformation($"[DEBUG] Registro de preço criado com sucesso: ID={registo.RegistoPrecoId}");
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = registo.RegistoPrecoId },
                    new ApiResponse<RegistosPreco>
                    {
                        Success = true,
                        Message = "Registro de preço criado com sucesso.",
                        StatusCode = 201,
                        Data = registo
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao criar registro de preço: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<RegistosPreco>
                {
                    Success = false,
                    Message = "Erro ao criar o registro de preço.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, RegistosPreco registo)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID do registro deve ser maior que zero.",
                    ErrorCode = "INVALID_ID",
                    StatusCode = 400,
                    Data = null
                });
            }

            _logger.LogInformation($"[DEBUG] Atualizando registro de preço com ID: {id}, Preço={registo.Preco}, ProdutoId={registo.ProdutoId}, LojaId={registo.LojaId}, TipoAcaoId={registo.TipoAcaoId}, UtilizadorId={registo.UtilizadorId}, Credibilidade={registo.Credibilidade}, DataRegisto={registo.DataRegisto}");
            try
            {
                if (id != registo.RegistoPrecoId)
                {
                    _logger.LogWarning($"[DEBUG] ID do registro ({registo.RegistoPrecoId}) não coincide com o ID da URL ({id}).");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ID do registro não coincide.",
                        ErrorCode = "INVALID_ID",
                        StatusCode = 400,
                        Data = null
                    });
                }

                if (registo.DataRegisto > DateTime.UtcNow)
                {
                    _logger.LogWarning("[DEBUG] Data de registro futura ao atualizar registro de preço.");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Data de registro não pode ser futura.",
                        ErrorCode = "INVALID_DATE",
                        StatusCode = 400,
                        Data = null
                    });
                }

                if (registo.Preco <= 0)
                {
                    _logger.LogWarning("[DEBUG] Preço inválido ao atualizar registro de preço.");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "O preço deve ser maior que zero.",
                        ErrorCode = "INVALID_DATA",
                        StatusCode = 400,
                        Data = null
                    });
                }

                if (registo.ProdutoId <= 0 || registo.LojaId <= 0 || registo.TipoAcaoId <= 0 || registo.UtilizadorId <= 0)
                {
                    _logger.LogWarning($"[DEBUG] Dados inválidos fornecidos - ProdutoId: {registo.ProdutoId}, LojaId: {registo.LojaId}, TipoAcaoId: {registo.TipoAcaoId}, UtilizadorId: {registo.UtilizadorId}");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "IDs devem ser maiores que zero.",
                        ErrorCode = "INVALID_DATA",
                        StatusCode = 400,
                        Data = null
                    });
                }

                bool prodExists = await _produtoRepository.ExistsAsync(registo.ProdutoId);
                if (!prodExists)
                {
                    _logger.LogWarning($"[DEBUG] Produto com ID {registo.ProdutoId} não existe.");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Produto {registo.ProdutoId} não existe.",
                        ErrorCode = "INVALID_PRODUCT",
                        StatusCode = 400,
                        Data = null
                    });
                }

                bool storeExists = await _lojaRepository.ExistsAsync(registo.LojaId);
                if (!storeExists)
                {
                    _logger.LogWarning($"[DEBUG] Loja com ID {registo.LojaId} não existe.");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Loja {registo.LojaId} não existe.",
                        ErrorCode = "INVALID_STORE",
                        StatusCode = 400,
                        Data = null
                    });
                }

                bool userExists = await _utilizadorRepository.ExistsAsync(registo.UtilizadorId);
                if (!userExists)
                {
                    _logger.LogWarning($"[DEBUG] Utilizador com ID {registo.UtilizadorId} não existe.");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Utilizador {registo.UtilizadorId} não existe.",
                        ErrorCode = "INVALID_USER",
                        StatusCode = 400,
                        Data = null
                    });
                }

                _logger.LogInformation("[DEBUG] Buscando registro existente...");
                var existingRegisto = await _registosPrecoRepository.GetByIdAsync(id);
                if (existingRegisto == null)
                {
                    _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado para atualização.");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Registro de preço não encontrado.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }
                _logger.LogInformation($"[DEBUG] Registro existente encontrado: ProdutoId={existingRegisto.ProdutoId}, LojaId={existingRegisto.LojaId}, UtilizadorId={existingRegisto.UtilizadorId}");

                // Garantir que propriedades de navegação não sejam salvas
                registo.Produto = null;
                registo.Loja = null;
                registo.TipoAcao = null;

                _logger.LogInformation("[DEBUG] Atualizando registro no repositório...");
                await _registosPrecoRepository.UpdateAsync(registo);
                _logger.LogInformation("[DEBUG] Registro atualizado com sucesso no repositório.");

                _logger.LogInformation("[DEBUG] Enviando notificação de mudança de preço...");
                await _hubContext.Clients.All.SendAsync("PriceChanged", registo.ProdutoId, registo.Preco);
                _logger.LogInformation($"[DEBUG] Notificação de preço enviada para ProdutoId: {registo.ProdutoId}");

                _logger.LogInformation($"[DEBUG] Registro de preço com ID {id} atualizado com sucesso.");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Registro de preço atualizado com sucesso.",
                    StatusCode = 200,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao atualizar registro de preço com ID {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erro ao atualizar o registro de preço.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID do registro deve ser maior que zero.",
                    ErrorCode = "INVALID_ID",
                    StatusCode = 400,
                    Data = null
                });
            }

            _logger.LogInformation($"[DEBUG] Deletando registro de preço com ID: {id}");
            try
            {
                var registo = await _registosPrecoRepository.GetByIdAsync(id);
                if (registo == null)
                {
                    _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado para deleção.");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Registro de preço não encontrado.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                await _registosPrecoRepository.DeleteAsync(registo);
                _logger.LogInformation($"[DEBUG] Registro de preço com ID {id} deletado com sucesso.");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Registro de preço removido com sucesso.",
                    StatusCode = 200,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao deletar registro de preço com ID {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erro ao deletar o registro de preço.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpPost("confirm/{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ConfirmPrice(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID do registro deve ser maior que zero.",
                    ErrorCode = "INVALID_ID",
                    StatusCode = 400,
                    Data = null
                });
            }

            _logger.LogInformation($"[DEBUG] Confirmando preço para o registro com ID: {id}");
            try
            {
                var registo = await _registosPrecoRepository.GetByIdAsync(id);
                if (registo == null)
                {
                    _logger.LogWarning($"[DEBUG] Registro de preço com ID {id} não encontrado para confirmação.");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Registro de preço não encontrado.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                registo.Credibilidade = Math.Min(registo.Credibilidade + 1, 10); // Ajustado para +1, limite 10
                registo.DataRegisto = DateTime.UtcNow;
                await _registosPrecoRepository.UpdateAsync(registo);

                // Incrementar pontos do utilizador
                var userIdClaim = User.FindFirst("utilizadorId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("[DEBUG] Usuário não identificado ao confirmar preço.");
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Usuário não identificado.",
                        ErrorCode = "UNAUTHORIZED",
                        StatusCode = 401,
                        Data = null
                    });
                }
                var utilizador = await _utilizadorRepository.GetByIdAsync(userId);
                if (utilizador != null)
                {
                    utilizador.Pontos += 2; // +2 pontos por confirmação
                    _logger.LogInformation($"[DEBUG] Novos pontos do utilizador {userId}: {utilizador.Pontos}");
                    await _utilizadorRepository.UpdateAsync(utilizador);
                    _logger.LogInformation($"[DEBUG] Pontos do utilizador {userId} atualizados com sucesso.");
                }
                else
                {
                    _logger.LogWarning($"[DEBUG] Utilizador {userId} não encontrado para incrementar pontos.");
                }

                _logger.LogInformation($"[DEBUG] Preço confirmado para o registro {id}, nova credibilidade: {registo.Credibilidade}");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Preço confirmado com sucesso.",
                    StatusCode = 200,
                    Data = new { Credibilidade = registo.Credibilidade }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERROR] Erro ao confirmar preço para o registro {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erro ao confirmar o preço.",
                    ErrorCode = "SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
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
