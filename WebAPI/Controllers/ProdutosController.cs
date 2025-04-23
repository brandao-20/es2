using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using WebAPI.Entities;
using WebAPI.Repositories;
using WebAPI.Extensions;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IRegistosPrecoRepository _registosPrecoRepository;

        public ProdutosController(
            IProdutoRepository produtoRepository,
            ICategoriaRepository categoriaRepository,
            IRegistosPrecoRepository registosPrecoRepository)
        {
            _produtoRepository = produtoRepository;
            _categoriaRepository = categoriaRepository;
            _registosPrecoRepository = registosPrecoRepository;
        }

        // GET: api/Produtos?page=1&pageSize=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var totalItems = await _produtoRepository.CountAsync();
            var skip = (page - 1) * pageSize;
            var produtos = await _produtoRepository.GetPagedWithDetailsAsync(skip, pageSize);

            Response.Headers["X-Total-Count"] = totalItems.ToString();
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _produtoRepository.GetByIdWithDetailsAsync(id);
            if (produto == null) return NotFound();
            return Ok(produto);
        }

        [HttpGet("{id}/credibilidade")]
        public async Task<ActionResult<object>> GetAdjustedCredibility(int id)
        {
            var produto = await _produtoRepository.GetByIdAsync(id);
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado." });

            var registos = await _registosPrecoRepository.GetByProdutoIdAsync(id);
            if (registos == null || !registos.Any())
                return Ok(new { Credibilidade = 0.0 });

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
            return Ok(new { Credibilidade = credibilidadeMedia });
        }

        [HttpPost]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<Produto>> Create(Produto produto)
        {
            if (string.IsNullOrEmpty(produto.Nome) || string.IsNullOrEmpty(produto.Marca))
                return BadRequest("Nome e Marca são obrigatórios.");

            bool categoryExists = await _categoriaRepository.ExistsAsync(produto.CategoriaId);
            if (!categoryExists)
                return BadRequest($"A categoria {produto.CategoriaId} não existe.");

            await _produtoRepository.AddAsync(produto);
            return CreatedAtAction(nameof(GetById), new { id = produto.ProdutoId }, produto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<IActionResult> Update(int id, Produto produto)
        {
            if (id != produto.ProdutoId)
                return BadRequest("ID do produto não coincide.");

            bool categoryExists = await _categoriaRepository.ExistsAsync(produto.CategoriaId);
            if (!categoryExists)
                return BadRequest($"A categoria {produto.CategoriaId} não existe.");

            try
            {
                await _produtoRepository.UpdateAsync(produto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Produto não encontrado.");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var produto = await _produtoRepository.GetByIdAsync(id);
            if (produto == null)
                return NotFound("Produto não encontrado.");

            await _produtoRepository.DeleteAsync(produto);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Produto>>> Search(
            [FromQuery] string? nome,
            [FromQuery] int? categoriaId,
            [FromQuery] string? store,
            [FromQuery] DateTime? dateFrom)
        {
            Console.WriteLine($"[DEBUG] Pesquisa de produtos - Nome: '{nome}', CategoriaId: {categoriaId}, Store: '{store}', DateFrom: {dateFrom}");
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
            return Ok(produtos);
        }
    }
}
