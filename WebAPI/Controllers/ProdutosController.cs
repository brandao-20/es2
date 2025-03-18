using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll()
        {
            var produtos = await _context.Produtos.Include(p => p.Categoria).ToListAsync();
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _context.Produtos.Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.ProdutoId == id);
            if (produto == null) return NotFound();
            return Ok(produto);
        }

        // Criação de produto – restrito a UserManager e Admin
        [HttpPost]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<ActionResult<Produto>> Create(Produto produto)
        {
            if (string.IsNullOrEmpty(produto.Nome) || string.IsNullOrEmpty(produto.Marca))
                return BadRequest("Nome e Marca são obrigatórios.");

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = produto.ProdutoId }, produto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<IActionResult> Update(int id, Produto produto)
        {
            if (id != produto.ProdutoId) return BadRequest("ID do produto não coincide.");

            _context.Entry(produto).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Produtos.Any(e => e.ProdutoId == id)) return NotFound("Produto não encontrado.");
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "UserManager,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound("Produto não encontrado.");

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Pesquisa de produtos (acesso público)
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Produto>>> Search(
            [FromQuery] string? nome,
            [FromQuery] int? categoriaId)
        {
            var query = _context.Produtos.Include(p => p.Categoria).AsQueryable();
            if (!string.IsNullOrEmpty(nome))
                query = query.Where(p => p.Nome.Contains(nome));
            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            var produtos = await query.ToListAsync();
            return Ok(produtos);
        }
    }
}
