using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RelatoriosController : ControllerBase
    {
        private readonly ILojaRepository _lojaRepository;
        private readonly IRegistosPrecoRepository _registosPrecoRepository;
        private readonly IProdutoRepository _produtoRepository;

        public RelatoriosController(
            ILojaRepository lojaRepository,
            IRegistosPrecoRepository registosPrecoRepository,
            IProdutoRepository produtoRepository)
        {
            _lojaRepository = lojaRepository;
            _registosPrecoRepository = registosPrecoRepository;
            _produtoRepository = produtoRepository;
        }

        // Relatório Geral de Lojas (Requisito 13)
        // Endpoint: GET /api/Relatorios/lojas
        [HttpGet("lojas")]
        public async Task<ActionResult<IEnumerable<LojaReportDto>>> GetLojasReport()
        {
            var lojas = await _lojaRepository.GetAllWithDetailsAsync();
            var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();
            var report = new List<LojaReportDto>();

            foreach (var loja in lojas)
            {
                // Para cada loja, agrupar os registos por produto (para obter o último preço)
                var produtosInfo = registos
                    .Where(r => r.LojaId == loja.LojaId)
                    .GroupBy(r => r.ProdutoId)
                    .Select(g => new ProdutoPriceDto
                    {
                        ProdutoId = g.Key,
                        ProdutoNome = g.Select(r => r.Produto.Nome).FirstOrDefault() ?? "N/A",
                        LatestPrice = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.Preco ?? 0,
                        LatestDate = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.DataRegisto ?? DateTime.MinValue
                    })
                    .ToList();

                var categoriaCounts = registos
                    .Where(r => r.LojaId == loja.LojaId)
                    .GroupBy(r => r.Produto.CategoriaId)
                    .Select(g => new CategoriaCountDto
                    {
                        CategoriaId = g.Key,
                        CategoriaNome = g.Select(r => r.Produto.Categoria.Nome).FirstOrDefault() ?? "N/A",
                        Count = g.Count()
                    })
                    .ToList();

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

        // Relatório Específico para uma Loja (Requisito 14)
        // Endpoint: GET /api/Relatorios/lojas/{lojaId}
        [HttpGet("lojas/{lojaId:int}")]
        public async Task<ActionResult<LojaReportDto>> GetLojaReport(int lojaId)
        {
            var loja = await _lojaRepository.GetByIdWithDetailsAsync(lojaId);
            if (loja == null)
                return NotFound($"Loja com ID {lojaId} não encontrada.");

            var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();

            var produtosInfo = registos
                .Where(r => r.LojaId == lojaId)
                .GroupBy(r => r.ProdutoId)
                .Select(g => new ProdutoPriceDto
                {
                    ProdutoId = g.Key,
                    ProdutoNome = g.Select(r => r.Produto.Nome).FirstOrDefault() ?? "N/A",
                    LatestPrice = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.Preco ?? 0,
                    LatestDate = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.DataRegisto ?? DateTime.MinValue
                })
                .ToList();

            var categoriaCounts = registos
                .Where(r => r.LojaId == lojaId)
                .GroupBy(r => r.Produto.CategoriaId)
                .Select(g => new CategoriaCountDto
                {
                    CategoriaId = g.Key,
                    CategoriaNome = g.Select(r => r.Produto.Categoria.Nome).FirstOrDefault() ?? "N/A",
                    Count = g.Count()
                })
                .ToList();

            var dto = new LojaReportDto
            {
                LojaId = loja.LojaId,
                Nome = loja.Nome,
                Endereco = loja.Endereco,
                Localizacao = loja.Localizacao,
                CategoriaCounts = categoriaCounts,
                Produtos = produtosInfo
            };

            return Ok(dto);
        }

        // Relatório Específico para um Produto (Requisito 15)
        // Endpoint: GET /api/Relatorios/produtos/{produtoId}
        [HttpGet("produtos/{produtoId:int}")]
        public async Task<ActionResult<ProdutoReportDto>> GetProdutoReport(int produtoId)
        {
            var produto = await _produtoRepository.GetByIdWithDetailsAsync(produtoId);
            if (produto == null)
                return NotFound($"Produto com ID {produtoId} não encontrado.");

            var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();

            var lojasInfo = registos
                .Where(r => r.ProdutoId == produtoId)
                .GroupBy(r => r.LojaId)
                .Select(g => new LojaPriceDto
                {
                    LojaId = g.Key,
                    LojaNome = g.Select(r => r.Loja.Nome).FirstOrDefault() ?? "N/A",
                    LatestPrice = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.Preco ?? 0,
                    LatestDate = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.DataRegisto ?? DateTime.MinValue
                })
                .ToList();

            var dto = new ProdutoReportDto
            {
                ProdutoId = produto.ProdutoId,
                Nome = produto.Nome,
                Categoria = produto.Categoria?.Nome,
                Lojas = lojasInfo
            };

            return Ok(dto);
        }
    }

    // DTOs para Relatórios

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
