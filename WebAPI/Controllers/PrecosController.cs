using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.DTOs;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrecosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PrecosController(AppDbContext context)
        {
            _context = context;
        }

        // Endpoint para listar todos os produtos
        [HttpGet("produtos")]
        public async Task<ActionResult<List<Produto>>> GetProdutos()
        {
            Console.WriteLine("[DEBUG] Acessando endpoint /api/precos/produtos");
            var produtos = await _context.Produtos
                .Select(p => new Produto
                {
                    ProdutoId = p.ProdutoId,
                    Nome = p.Nome,
                    Marca = p.Marca
                })
                .OrderBy(p => p.Nome)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Produtos encontrados: {produtos.Count}");
            return Ok(produtos);
        }

        // Endpoint para comparar preços de um produto
        [HttpGet("comparar/{produtoId}")]
        public async Task<ActionResult<PrecoComparacaoDTO>> CompararPrecos(int produtoId, [FromQuery] int? lojaId1 = null, [FromQuery] int? lojaId2 = null)
        {
            Console.WriteLine($"[DEBUG] Acessando endpoint /api/precos/comparar/{produtoId} com lojaId1={lojaId1}, lojaId2={lojaId2}");

            // Verifica se o produto existe
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.ProdutoId == produtoId);

            if (produto == null)
            {
                Console.WriteLine($"[DEBUG] Produto com ID {produtoId} não encontrado.");
                return NotFound("Produto não encontrado.");
            }

            // Obtém as lojas disponíveis para este produto
            var lojasDisponiveis = await _context.RegistosPrecos
                .Where(r => r.ProdutoId == produtoId)
                .Select(r => r.Loja)
                .Distinct()
                .Select(l => new WebAPI.DTOs.LojaDTO
                {
                    LojaId = l.LojaId,
                    Nome = l.Nome
                })
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Lojas disponíveis para o produto {produtoId}: {lojasDisponiveis.Count}");

            // Filtra os registos de preços com base nas lojas selecionadas (se fornecidas)
            IQueryable<RegistosPreco> query = _context.RegistosPrecos
                .Where(r => r.ProdutoId == produtoId);

            if (lojaId1.HasValue && lojaId2.HasValue)
            {
                query = query.Where(r => r.LojaId == lojaId1.Value || r.LojaId == lojaId2.Value);
                Console.WriteLine($"[DEBUG] Filtrando preços para lojas {lojaId1.Value} e {lojaId2.Value}");
            }

            // Obtém os preços mais recentes por loja
            var precosAtuais = await query
                .GroupBy(r => r.LojaId)
                .Select(g => g.OrderByDescending(r => r.DataRegisto).FirstOrDefault())
                .Include(r => r.Loja)
                .Select(r => new PrecoAtualDTO
                {
                    NomeLoja = r.Loja.Nome,
                    Preco = r.Preco,
                    DataRegisto = r.DataRegisto
                })
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Preços atuais encontrados: {precosAtuais.Count}");

            // Obtém o histórico de preços
            var historicoPrecos = await query
                .Include(r => r.Loja)
                .Select(r => new HistoricoPrecoDTO
                {
                    NomeLoja = r.Loja.Nome,
                    Preco = r.Preco,
                    DataRegisto = r.DataRegisto
                })
                .OrderBy(r => r.DataRegisto)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Histórico de preços encontrados: {historicoPrecos.Count}");

            var resultado = new PrecoComparacaoDTO
            {
                NomeProduto = produto.Nome,
                LojasDisponiveis = lojasDisponiveis,
                PrecosAtuais = precosAtuais,
                HistoricoPrecos = historicoPrecos
            };

            return Ok(resultado);
        }
    }
}
