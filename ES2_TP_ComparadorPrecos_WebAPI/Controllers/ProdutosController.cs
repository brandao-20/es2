using ES2_TP_ComparadorPrecos_WebAPI.Context;
using ES2_TP_ComparadorPrecos_WebAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2_TP_ComparadorPrecos_WebAPI.Controllers
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

        // GET /api/Produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll()
        {
            var produtos = await _context.Produtos
                .Include(p => p.Categoria)
                .ToListAsync();
            return Ok(produtos);
        }

        // GET /api/Produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.ProdutoId == id);

            if (produto == null) return NotFound();
            return Ok(produto);
        }

        // POST /api/Produtos
        [HttpPost]
        public async Task<ActionResult<Produto>> Create(Produto produto)
        {
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = produto.ProdutoId }, produto);
        }

        // PUT /api/Produtos/5
        [HttpPut("{id}")]
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
                if (!Exists(id)) return NotFound("Produto não encontrado.");
                throw;
            }

            return NoContent();
        }

        // DELETE /api/Produtos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound("Produto não encontrado.");

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET /api/Produtos/search?nome=...
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Produto>>> Search(
            [FromQuery] string? nome,
            [FromQuery] int? categoriaId,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim)
        {
            var query = _context.Produtos
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(p => p.Nome.Contains(nome));
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            var produtos = await query.ToListAsync();
            return Ok(produtos);
        }

        private bool Exists(int id)
        {
            return _context.Produtos.Any(e => e.ProdutoId == id);
        }
    }
}
