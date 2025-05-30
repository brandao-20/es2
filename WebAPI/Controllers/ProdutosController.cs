using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using WebAPI.Entities;
using WebAPI.Repositories;
using WebAPI.Extensions;
using WebAPI.Helpers;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IRegistosPrecoRepository _registosPrecoRepository;
        private readonly ILogger<ProdutosController> _logger;

        public ProdutosController(
            IProdutoRepository produtoRepository,
            ICategoriaRepository categoriaRepository,
            IRegistosPrecoRepository registosPrecoRepository,
            ILogger<ProdutosController> logger)
        {
            _produtoRepository = produtoRepository;
            _categoriaRepository = categoriaRepository;
            _registosPrecoRepository = registosPrecoRepository;
            _logger = logger;
        }

        // GET: api/Produtos?page=1&pageSize=5
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Produto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 5;

                _logger.LogInformation($"[DEBUG] Obtendo produtos - Página: {page}, Tamanho da página: {pageSize}");

                var totalItems = await _produtoRepository.CountAsync();
                var skip = (page - 1) * pageSize;
                var produtos = await _produtoRepository.GetPagedWithDetailsAsync(skip, pageSize);

                Response.Headers["X-Total-Count"] = totalItems.ToString();
                return Ok(new ApiResponse<IEnumerable<Produto>>
                {
                    Success = true,
                    Message = "Produtos obtidos com sucesso.",
                    StatusCode = 200,
                    Data = produtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Erro ao obter produtos - Página: {Page}, Tamanho da página: {PageSize}", page, pageSize);
                return StatusCode(500, new ApiResponse<IEnumerable<Produto>>
                {
                    Success = false,
                    Message = $"Erro ao obter produtos: {ex.Message}",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Produto>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                    return BadRequest(new ApiResponse<Produto>
                    {
                        Success = false,
                        Message = "ID do produto deve ser maior que zero.",
                        ErrorCode = "INVALID_ID",
                        StatusCode = 400,
                        Data = null
                    });
                }

                _logger.LogInformation($"[DEBUG] Obtendo produto com ID: {id}");

                var produto = await _produtoRepository.GetByIdWithDetailsAsync(id);
                if (produto == null)
                {
                    _logger.LogWarning($"[DEBUG] Produto com ID {id} não encontrado.");
                    return NotFound(new ApiResponse<Produto>
                    {
                        Success = false,
                        Message = "Produto não encontrado.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<Produto>
                {
                    Success = true,
                    Message = "Produto obtido com sucesso.",
                    StatusCode = 200,
                    Data = produto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Erro ao obter produto com ID: {Id}", id);
                return StatusCode(500, new ApiResponse<Produto>
                {
                    Success = false,
                    Message = $"Erro ao obter produto: {ex.Message}",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpGet("{id}/credibilidade")]
        public async Task<ActionResult<ApiResponse<object>>> GetAdjustedCredibility(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ID do produto deve ser maior que zero.",
                        ErrorCode = "INVALID_ID",
                        StatusCode = 400,
                        Data = null
                    });
                }

                _logger.LogInformation($"[DEBUG] Calculando credibilidade ajustada para o produto com ID: {id}");

                var produto = await _produtoRepository.GetByIdAsync(id);
                if (produto == null)
                {
                    _logger.LogWarning($"[DEBUG] Produto com ID {id} não encontrado.");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Produto não encontrado.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                var registos = await _registosPrecoRepository.GetByProdutoIdAsync(id);
                if (registos == null || !registos.Any())
                {
                    _logger.LogInformation($"[DEBUG] Nenhum registo de preço encontrado para o produto com ID: {id}");
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Nenhum registo de preço encontrado.",
                        StatusCode = 200,
                        Data = new { Credibilidade = 0.0 }
                    });
                }

                double totalCredibilidade = 0;
                int count = 0;

                foreach (var registo in registos)
                {
                    var meses = (DateTime.UtcNow - registo.DataRegisto).TotalDays / 30;
                    var adjusted = (double)registo.Credibilidade - (0.1 * meses);
                    if (adjusted < 0) adjusted = 0;

                    totalCredibilidade += adjusted;
                    count++;
                }

                var credibilidadeMedia = count > 0 ? totalCredibilidade / count : 0;
                _logger.LogInformation($"[DEBUG] Credibilidade ajustada calculada para o produto {id}: {credibilidadeMedia}");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Credibilidade ajustada calculada com sucesso.",
                    StatusCode = 200,
                    Data = new { Credibilidade = credibilidadeMedia }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Erro ao calcular credibilidade ajustada para o produto com ID: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Erro ao calcular credibilidade: {ex.Message}",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<ApiResponse<Produto>>> Create(Produto produto)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] Criando novo produto: Nome={produto.Nome}, Marca={produto.Marca}, CategoriaId={produto.CategoriaId}");

                if (string.IsNullOrEmpty(produto.Nome) || string.IsNullOrWhiteSpace(produto.Marca))
                {
                    _logger.LogWarning($"[DEBUG] Nome ou Marca vazios ao criar produto.");
                    return BadRequest(new ApiResponse<Produto>
                    {
                        Success = false,
                        Message = "Nome e Marca são obrigatórios.",
                        ErrorCode = "INVALID_DATA",
                        StatusCode = 400,
                        Data = null
                    });
                }

                bool categoryExists = await _categoriaRepository.ExistsAsync(produto.CategoriaId);
                if (!categoryExists)
                {
                    _logger.LogWarning($"[DEBUG] Categoria com ID {produto.CategoriaId} não existe.");
                    return BadRequest(new ApiResponse<Produto>
                    {
                        Success = false,
                        Message = $"A categoria {produto.CategoriaId} não existe.",
                        ErrorCode = "INVALID_CATEGORY",
                        StatusCode = 400,
                        Data = null
                    });
                }

                await _produtoRepository.AddAsync(produto);
                _logger.LogInformation($"[DEBUG] Produto criado com sucesso: ID={produto.ProdutoId}");
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = produto.ProdutoId },
                    new ApiResponse<Produto>
                    {
                        Success = true,
                        Message = "Produto criado com sucesso.",
                        StatusCode = 201,
                        Data = produto
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Erro ao criar produto: Nome={Nome}, Marca={Marca}, CategoriaId={CategoriaId}", produto.Nome, produto.Marca, produto.CategoriaId);
                return StatusCode(500, new ApiResponse<Produto>
                {
                    Success = false,
                    Message = $"Erro ao criar produto: {ex.Message}",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, Produto produto)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ID do produto deve ser maior que zero.",
                        ErrorCode = "INVALID_ID",
                        StatusCode = 400,
                        Data = null
                    });
                }

                _logger.LogInformation($"[DEBUG] Atualizando produto com ID: {id}, Nome={produto.Nome}, Marca={produto.Marca}, CategoriaId={produto.CategoriaId}");

                if (id != produto.ProdutoId)
                {
                    _logger.LogWarning($"[DEBUG] ID do produto ({produto.ProdutoId}) não coincide com o ID da URL ({id}).");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ID do produto não coincide.",
                        ErrorCode = "INVALID_ID",
                        StatusCode = 400,
                        Data = null
                    });
                }

                bool categoryExists = await _categoriaRepository.ExistsAsync(produto.CategoriaId);
                if (!categoryExists)
                {
                    _logger.LogWarning($"[DEBUG] Categoria com ID {produto.CategoriaId} não existe.");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"A categoria {produto.CategoriaId} não existe.",
                        ErrorCode = "INVALID_CATEGORY",
                        StatusCode = 400,
                        Data = null
                    });
                }

                await _produtoRepository.UpdateAsync(produto);
                _logger.LogInformation($"[DEBUG] Produto com ID {id} atualizado com sucesso.");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Produto atualizado com sucesso.",
                    StatusCode = 200,
                    Data = null
                });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning($"[DEBUG] Produto com ID {id} não encontrado para atualização.");
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Produto não encontrado.",
                    ErrorCode = "NOT_FOUND",
                    StatusCode = 404,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Erro ao atualizar produto com ID: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Erro ao atualizar produto: {ex.Message}",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ID do produto deve ser maior que zero.",
                        ErrorCode = "INVALID_ID",
                        StatusCode = 400,
                        Data = null
                    });
                }

                _logger.LogInformation($"[DEBUG] Deletando produto com ID: {id}");

                var produto = await _produtoRepository.GetByIdAsync(id);
                if (produto == null)
                {
                    _logger.LogWarning($"[DEBUG] Produto com ID {id} não encontrado para deleção.");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Produto não encontrado.",
                        ErrorCode = "NOT_FOUND",
                        StatusCode = 404,
                        Data = null
                    });
                }

                await _produtoRepository.DeleteAsync(produto);
                _logger.LogInformation($"[DEBUG] Produto com ID {id} deletado com sucesso.");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Produto removido com sucesso.",
                    StatusCode = 200,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Erro ao deletar produto com ID: {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Erro ao deletar produto: {ex.Message}",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Produto>>>> Search(
            [FromQuery] string? nome,
            [FromQuery] int? categoriaId,
            [FromQuery] string? store,
            [FromQuery] DateTime? dateFrom)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] Pesquisa de produtos - Nome: '{nome}', CategoriaId: {categoriaId}, Store: '{store}', DateFrom: {dateFrom}");

                Expression<Func<Produto, bool>> predicate = p => true;

                if (!string.IsNullOrWhiteSpace(nome))
                {
                    string nomeLower = nome.ToLower();
                    predicate = p => p.Nome.ToLower().Contains(nomeLower);
                }

                if (categoriaId.HasValue)
                {
                    Expression<Func<Produto, bool>> categoriaPredicate = p => p.CategoriaId == categoriaId.Value;
                    predicate = predicate.And(categoriaPredicate);
                }

                if (!string.IsNullOrWhiteSpace(store))
                {
                    Expression<Func<Produto, bool>> storePredicate = p =>
                        p.RegistosPrecos.Any(rp => rp.Loja != null && rp.Loja.Nome.ToLower().Contains(store.ToLower()));
                    predicate = predicate.And(storePredicate);
                }

                if (dateFrom.HasValue)
                {
                    Expression<Func<Produto, bool>> datePredicate = p =>
                        p.RegistosPrecos.Any(rp => rp.DataRegisto >= dateFrom.Value);
                    predicate = predicate.And(datePredicate);
                }

                var produtos = await _produtoRepository.FindWithDetailsAsync(predicate);
                _logger.LogInformation($"[DEBUG] Pesquisa retornou {produtos.Count()} produtos.");
                return Ok(new ApiResponse<IEnumerable<Produto>>
                {
                    Success = true,
                    Message = "Pesquisa realizada com sucesso.",
                    StatusCode = 200,
                    Data = produtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] Erro ao pesquisar produtos - Nome: {Nome}, CategoriaId: {CategoriaId}, Store: {Store}, DateFrom: {DateFrom}", nome, categoriaId, store, dateFrom);
                return StatusCode(500, new ApiResponse<IEnumerable<Produto>>
                {
                    Success = false,
                    Message = $"Erro ao pesquisar produtos: {ex.Message}",
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    StatusCode = 500,
                    Data = null
                });
            }
        }
    }
}
