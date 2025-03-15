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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll()
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.ProdutoId == id);

            if (produto == null) return NotFound();
            return produto;
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> Create(Produto produto)
        {
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = produto.ProdutoId }, produto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Produto produto)
        {
            if (id != produto.ProdutoId) return BadRequest();

            _context.Entry(produto).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.Produtos.Any(e => e.ProdutoId == id);
        }
    }
}
