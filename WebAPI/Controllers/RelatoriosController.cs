using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RelatoriosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RelatoriosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("lojas")]
        public async Task<ActionResult<IEnumerable<LojaReportDto>>> GetLojasReport()
        {
            var lojas = await _context.Lojas.Include(l => l.Localizacao).ToListAsync();
            var report = new List<LojaReportDto>();

            foreach (var loja in lojas)
            {
                var produtosInfo = await _context.RegistosPrecos
                    .Where(r => r.LojaId == loja.LojaId)
                    .GroupBy(r => r.ProdutoId)
                    .Select(g => new ProdutoPriceDto
                    {
                        ProdutoId = g.Key,
                        ProdutoNome = g.Select(r => r.Produto.Nome).FirstOrDefault(),
                        LatestPrice = g.OrderByDescending(r => r.DataRegisto).Select(r => r.Preco).FirstOrDefault(),
                        LatestDate = g.OrderByDescending(r => r.DataRegisto).Select(r => r.DataRegisto).FirstOrDefault()
                    })
                    .ToListAsync();

                var categoriaCounts = await _context.RegistosPrecos
                    .Where(r => r.LojaId == loja.LojaId)
                    .GroupBy(r => r.Produto.CategoriaId)
                    .Select(g => new CategoriaCountDto
                    {
                        CategoriaId = g.Key,
                        CategoriaNome = g.Select(r => r.Produto.Categoria.Nome).FirstOrDefault(),
                        Count = g.Count()
                    })
                    .ToListAsync();

                report.Add(new LojaReportDto
                {
                    LojaId = loja.LojaId,
                    Nome = loja.Nome,
                    Endereco = loja.Endereco,
                    Localizacao = loja.Localizacao,
                    CategoriaCounts = categoriaCounts,
                    Produtos = produtosInfo
                });
            }

            return Ok(report);
        }

        [HttpGet("produtos")]
        public async Task<ActionResult<IEnumerable<ProdutoReportDto>>> GetProdutosReport()
        {
            var produtos = await _context.Produtos.Include(p => p.Categoria).ToListAsync();
            var report = new List<ProdutoReportDto>();

            foreach (var produto in produtos)
            {
                var lojasInfo = await _context.RegistosPrecos
                    .Where(r => r.ProdutoId == produto.ProdutoId)
                    .GroupBy(r => r.LojaId)
                    .Select(g => new LojaPriceDto
                    {
                        LojaId = g.Key,
                        LojaNome = g.Select(r => r.Loja.Nome).FirstOrDefault(),
                        LatestPrice = g.OrderByDescending(r => r.DataRegisto).Select(r => r.Preco).FirstOrDefault(),
                        LatestDate = g.OrderByDescending(r => r.DataRegisto).Select(r => r.DataRegisto).FirstOrDefault()
                    })
                    .ToListAsync();

                report.Add(new ProdutoReportDto
                {
                    ProdutoId = produto.ProdutoId,
                    Nome = produto.Nome,
                    Categoria = produto.Categoria?.Nome,
                    Lojas = lojasInfo
                });
            }

            return Ok(report);
        }
    }

    public class LojaReportDto
    {
        public int LojaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public Localizacao? Localizacao { get; set; }
        public List<CategoriaCountDto> CategoriaCounts { get; set; } = new();
        public List<ProdutoPriceDto> Produtos { get; set; } = new();
    }

    public class CategoriaCountDto
    {
        public int CategoriaId { get; set; }
        public string? CategoriaNome { get; set; }
        public int Count { get; set; }
    }

    public class ProdutoPriceDto
    {
        public int ProdutoId { get; set; }
        public string? ProdutoNome { get; set; }
        public decimal LatestPrice { get; set; }
        public DateTime LatestDate { get; set; }
    }

    public class ProdutoReportDto
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public List<LojaPriceDto> Lojas { get; set; } = new();
    }

    public class LojaPriceDto
    {
        public int LojaId { get; set; }
        public string? LojaNome { get; set; }
        public decimal LatestPrice { get; set; }
        public DateTime LatestDate { get; set; }
    }
}
