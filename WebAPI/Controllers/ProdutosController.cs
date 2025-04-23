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
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            _logger.LogInformation($"[DEBUG] Obtendo produtos - Página: {page}, Tamanho da página: {pageSize}");

            var totalItems = await _produtoRepository.CountAsync();
            var skip = (page - 1) * pageSize;
            var produtos = await _produtoRepository.GetPagedWithDetailsAsync(skip, pageSize);

            Response.Headers["X-Total-Count"] = totalItems.ToString();
            return Ok(ApiResponse<IEnumerable<Produto>>.SuccessResponse(produtos, "Produtos obtidos com sucesso."));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Produto>>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<Produto>.ErrorResponse("ID do produto deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Obtendo produto com ID: {id}");

            var produto = await _produtoRepository.GetByIdWithDetailsAsync(id);
            if (produto == null)
            {
                _logger.LogWarning($"[DEBUG] Produto com ID {id} não encontrado.");
                return NotFound(ApiResponse<Produto>.ErrorResponse("Produto não encontrado.", "NOT_FOUND", 404));
            }

            return Ok(ApiResponse<Produto>.SuccessResponse(produto, "Produto obtido com sucesso."));
        }

        [HttpGet("{id}/credibilidade")]
        public async Task<ActionResult<ApiResponse<object>>> GetAdjustedCredibility(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do produto deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Calculando credibilidade ajustada para o produto com ID: {id}");

            var produto = await _produtoRepository.GetByIdAsync(id);
            if (produto == null)
            {
                _logger.LogWarning($"[DEBUG] Produto com ID {id} não encontrado.");
                return NotFound(ApiResponse<object>.ErrorResponse("Produto não encontrado.", "NOT_FOUND", 404));
            }

            var registos = await _registosPrecoRepository.GetByProdutoIdAsync(id);
            if (registos == null || !registos.Any())
            {
                _logger.LogInformation($"[DEBUG] Nenhum registo de preço encontrado para o produto com ID: {id}");
                return Ok(ApiResponse<object>.SuccessResponse(new { Credibilidade = 0.0 }, "Nenhum registo de preço encontrado."));
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
            return Ok(ApiResponse<object>.SuccessResponse(new { Credibilidade = credibilidadeMedia }, "Credibilidade ajustada calculada com sucesso."));
        }

        [HttpPost]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<ApiResponse<Produto>>> Create(Produto produto)
        {
            _logger.LogInformation($"[DEBUG] Criando novo produto: Nome={produto.Nome}, Marca={produto.Marca}, CategoriaId={produto.CategoriaId}");

            if (string.IsNullOrEmpty(produto.Nome) || string.IsNullOrWhiteSpace(produto.Marca))
            {
                _logger.LogWarning($"[DEBUG] Nome ou Marca vazios ao criar produto.");
                return BadRequest(ApiResponse<Produto>.ErrorResponse("Nome e Marca são obrigatórios.", "INVALID_DATA", 400));
            }

            bool categoryExists = await _categoriaRepository.ExistsAsync(produto.CategoriaId);
            if (!categoryExists)
            {
                _logger.LogWarning($"[DEBUG] Categoria com ID {produto.CategoriaId} não existe.");
                return BadRequest(ApiResponse<Produto>.ErrorResponse($"A categoria {produto.CategoriaId} não existe.", "INVALID_CATEGORY", 400));
            }

            await _produtoRepository.AddAsync(produto);
            _logger.LogInformation($"[DEBUG] Produto criado com sucesso: ID={produto.ProdutoId}");
            return CreatedAtAction(
                nameof(GetById),
                new { id = produto.ProdutoId },
                ApiResponse<Produto>.SuccessResponse(produto, "Produto criado com sucesso.")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, Produto produto)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do produto deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Atualizando produto com ID: {id}, Nome={produto.Nome}, Marca={produto.Marca}, CategoriaId={produto.CategoriaId}");

            if (id != produto.ProdutoId)
            {
                _logger.LogWarning($"[DEBUG] ID do produto ({produto.ProdutoId}) não coincide com o ID da URL ({id}).");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do produto não coincide.", "INVALID_ID", 400));
            }

            bool categoryExists = await _categoriaRepository.ExistsAsync(produto.CategoriaId);
            if (!categoryExists)
            {
                _logger.LogWarning($"[DEBUG] Categoria com ID {produto.CategoriaId} não existe.");
                return BadRequest(ApiResponse<object>.ErrorResponse($"A categoria {produto.CategoriaId} não existe.", "INVALID_CATEGORY", 400));
            }

            try
            {
                await _produtoRepository.UpdateAsync(produto);
                _logger.LogInformation($"[DEBUG] Produto com ID {id} atualizado com sucesso.");
                return Ok(ApiResponse<object>.SuccessResponse(null, "Produto atualizado com sucesso."));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning($"[DEBUG] Produto com ID {id} não encontrado para atualização.");
                return NotFound(ApiResponse<object>.ErrorResponse("Produto não encontrado.", "NOT_FOUND", 404));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[DEBUG] ID inválido fornecido: {id}");
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do produto deve ser maior que zero.", "INVALID_ID", 400));
            }

            _logger.LogInformation($"[DEBUG] Deletando produto com ID: {id}");

            var produto = await _produtoRepository.GetByIdAsync(id);
            if (produto == null)
            {
                _logger.LogWarning($"[DEBUG] Produto com ID {id} não encontrado para deleção.");
                return NotFound(ApiResponse<object>.ErrorResponse("Produto não encontrado.", "NOT_FOUND", 404));
            }

            await _produtoRepository.DeleteAsync(produto);
            _logger.LogInformation($"[DEBUG] Produto com ID {id} deletado com sucesso.");
            return Ok(ApiResponse<object>.SuccessResponse(null, "Produto removido com sucesso."));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Produto>>>> Search(
            [FromQuery] string? nome,
            [FromQuery] int? categoriaId,
            [FromQuery] string? store,
            [FromQuery] DateTime? dateFrom)
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
                    p.RegistosPrecos.Any(rp => rp.Loja.Nome.ToLower().Contains(store.ToLower()));
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
            return Ok(ApiResponse<IEnumerable<Produto>>.SuccessResponse(produtos, "Pesquisa realizada com sucesso."));
        }
    }
}
