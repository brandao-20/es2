using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Context;
using WebAPI.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorito([FromBody] Favorito favorito)
        {
            Console.WriteLine($"[DEBUG] Recebendo requisição para favoritar/desfavoritar: ProdutoId={favorito.ProdutoId}, UtilizadorId={favorito.UtilizadorId}");
            try
            {
                // Validar campos obrigatórios
                if (favorito.ProdutoId <= 0)
                {
                    Console.WriteLine("[DEBUG] ProdutoId inválido.");
                    return BadRequest(new { Success = false, Message = "ProdutoId deve ser maior que zero." });
                }

                if (favorito.UtilizadorId <= 0)
                {
                    Console.WriteLine("[DEBUG] UtilizadorId inválido.");
                    return BadRequest(new { Success = false, Message = "UtilizadorId deve ser maior que zero." });
                }

                var userIdClaim = User.FindFirst("utilizadorId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    Console.WriteLine("[ERROR] Utilizador não identificado na requisição.");
                    return Unauthorized(new { Success = false, Message = "Utilizador não identificado." });
                }

                // Garantir que o UtilizadorId vem do token JWT
                favorito.UtilizadorId = userId;
                Console.WriteLine($"[DEBUG] Utilizador autenticado: {userId}");

                var exists = await _context.Favoritos
                    .AnyAsync(f => f.UtilizadorId == userId && f.ProdutoId == favorito.ProdutoId);
                Console.WriteLine($"[DEBUG] Produto já está favoritado: {exists}");

                if (exists)
                {
                    var favoritoToRemove = await _context.Favoritos
                        .FirstOrDefaultAsync(f => f.UtilizadorId == userId && f.ProdutoId == favorito.ProdutoId);
                    if (favoritoToRemove == null)
                    {
                        Console.WriteLine("[ERROR] Favorito não encontrado para remoção.");
                        return NotFound(new { Success = false, Message = "Favorito não encontrado." });
                    }
                    _context.Favoritos.Remove(favoritoToRemove);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("[DEBUG] Produto removido dos favoritos.");
                    return Ok(new { Success = true, Message = "Produto removido dos favoritos." });
                }
                else
                {
                    var produtoExists = await _context.Produtos.AnyAsync(p => p.ProdutoId == favorito.ProdutoId);
                    if (!produtoExists)
                    {
                        Console.WriteLine($"[DEBUG] Produto com ID {favorito.ProdutoId} não existe.");
                        return BadRequest(new { Success = false, Message = "Produto não encontrado." });
                    }

                    favorito.Produto = null; // Garantir que não tentamos salvar o objeto Produto
                    favorito.Utilizador = null; // Garantir que não tentamos salvar o objeto Utilizador
                    _context.Favoritos.Add(favorito);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("[DEBUG] Produto adicionado aos favoritos.");
                    return Ok(new { Success = true, Message = "Produto adicionado aos favoritos." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao processar favorito: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Success = false, Message = "Erro ao processar o favorito." });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetFavoritos(int userId)
        {
            Console.WriteLine($"[DEBUG] Buscando favoritos para o usuário {userId}");
            try
            {
                var currentUserIdClaim = User.FindFirst("utilizadorId")?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    Console.WriteLine("[ERROR] Usuário atual não identificado.");
                    return Unauthorized(new { Success = false, Message = "Usuário atual não identificado." });
                }

                if (currentUserId != userId)
                {
                    Console.WriteLine($"[ERROR] Usuário {currentUserId} tentou acessar os favoritos do usuário {userId}.");
                    return Unauthorized(new { Success = false, Message = "Acesso não autorizado." });
                }

                var favoritos = await _context.Favoritos
                    .Where(f => f.UtilizadorId == userId)
                    .Include(f => f.Produto)
                    .Select(f => new
                    {
                        f.FavoritoId,
                        f.ProdutoId,
                        ProdutoNome = f.Produto != null ? f.Produto.Nome : "Produto Desconhecido"
                    })
                    .ToListAsync();

                Console.WriteLine($"[DEBUG] Favoritos encontrados: {favoritos.Count}");
                return Ok(new { Success = true, Data = favoritos });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao buscar favoritos: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Success = false, Message = "Erro ao buscar os favoritos." });
            }
        }
    }
}
