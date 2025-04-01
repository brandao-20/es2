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

        public ProdutosController(
            IProdutoRepository produtoRepository,
            ICategoriaRepository categoriaRepository)
        {
            _produtoRepository = produtoRepository;
            _categoriaRepository = categoriaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll()
        {
            var produtos = await _produtoRepository.GetAllWithDetailsAsync();
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _produtoRepository.GetByIdWithDetailsAsync(id);
            if (produto == null) return NotFound();
            return Ok(produto);
        }

        [HttpPost]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<Produto>> Create(Produto produto)
        {
            if (string.IsNullOrEmpty(produto.Nome) || string.IsNullOrEmpty(produto.Marca))
                return BadRequest("Nome e Marca são obrigatórios.");

            bool categoryExists = await _categoriaRepository.ExistsAsync(produto.CategoriaId);
            if (!categoryExists)
            {
                return BadRequest($"A categoria {produto.CategoriaId} não existe.");
            }

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
            {
                return BadRequest($"A categoria {produto.CategoriaId} não existe.");
            }

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
            // Log para depurar os valores recebidos
            Console.WriteLine($"[DEBUG] Pesquisa de produtos - Nome: '{nome}', CategoriaId: {categoriaId}, Store: '{store}', DateFrom: {dateFrom}");

            // Criar a expressão de filtro
            Expression<Func<Produto, bool>> predicate = p => true; // Começa com um filtro que aceita todos os produtos

            // Adicionar filtro por nome, se fornecido
            if (!string.IsNullOrWhiteSpace(nome))
            {
                string nomeLower = nome.ToLower();
                predicate = p => p.Nome.ToLower().Contains(nomeLower);
            }

            // Adicionar filtro por categoria, se fornecido
            if (categoriaId.HasValue)
            {
                Expression<Func<Produto, bool>> categoriaPredicate = p => p.CategoriaId == categoriaId.Value;
                predicate = predicate.And(categoriaPredicate);
            }

            // Adicionar filtro por loja, se fornecido (requer join com RegistosPreco e Loja)
            if (!string.IsNullOrWhiteSpace(store))
            {
                Expression<Func<Produto, bool>> storePredicate = p =>
                    p.RegistosPrecos.Any(rp => rp.Loja.Nome.ToLower().Contains(store.ToLower()));
                predicate = predicate.And(storePredicate);
            }

            // Adicionar filtro por data, se fornecido (requer join com RegistosPreco)
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
