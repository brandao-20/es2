using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistosPrecosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistosPrecosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistosPreco>>> GetAll()
        {
            return await _context.RegistosPrecos
                .Include(r => r.Produto)
                .Include(r => r.Loja)
                .Include(r => r.Utilizador)
                .Include(r => r.TipoAcao)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegistosPreco>> GetById(int id)
        {
            var registo = await _context.RegistosPrecos
                .Include(r => r.Produto)
                .Include(r => r.Loja)
                .Include(r => r.Utilizador)
                .Include(r => r.TipoAcao)
                .FirstOrDefaultAsync(r => r.RegistoPrecoId == id);

            if (registo == null) return NotFound();
            return registo;
        }

        // Criação de registo de preço – usuário autenticado
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RegistosPreco>> Create(RegistosPreco registo)
        {
            try
            {
                // Checar se produto e loja existem
                bool prodExists = await _context.Produtos.AnyAsync(p => p.ProdutoId == registo.ProdutoId);
                if (!prodExists) return BadRequest($"Produto {registo.ProdutoId} não existe.");

                bool storeExists = await _context.Lojas.AnyAsync(l => l.LojaId == registo.LojaId);
                if (!storeExists) return BadRequest($"Loja {registo.LojaId} não existe.");

                // Se quiser, define dataRegisto
                if (registo.DataRegisto == default)
                    registo.DataRegisto = DateTime.UtcNow;

                _context.RegistosPrecos.Add(registo);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = registo.RegistoPrecoId }, registo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, "Ocorreu um erro interno: " + ex.Message);
            }
        }

        // Atualização de preço – usuário autenticado
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, RegistosPreco registo)
        {
            if (id != registo.RegistoPrecoId) return BadRequest();

            _context.Entry(registo).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RegistosPrecos.Any(e => e.RegistoPrecoId == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var registo = await _context.RegistosPrecos.FindAsync(id);
            if (registo == null) return NotFound();

            _context.RegistosPrecos.Remove(registo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Novo endpoint para confirmação de preço, atualizando a credibilidade
        [HttpPost("confirm/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmPrice(int id)
        {
            var registo = await _context.RegistosPrecos.FindAsync(id);
            if (registo == null) return NotFound();

            // Exemplo simples: incrementar a credibilidade em 10 pontos (até máximo 100)
            registo.Credibilidade = Math.Min(registo.Credibilidade + 10, 100);
            _context.Entry(registo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Preço confirmado com sucesso", credibilidade = registo.Credibilidade });
        }
    }
}
