using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;
using WebAPI.ExportStrategies;
using WebAPI.Repositories;
using WebAPI.DTOs;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RelatoriosController : ControllerBase
    {
        private readonly ILojaRepository _lojaRepository;
        private readonly IRegistosPrecoRepository _registosPrecoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly ReportExportService _exportService;
        private readonly AppDbContext _context;

        public RelatoriosController(
            ILojaRepository lojaRepository,
            IRegistosPrecoRepository registosPrecoRepository,
            IProdutoRepository produtoRepository,
            ReportExportService exportService,
            AppDbContext context)
        {
            _lojaRepository = lojaRepository;
            _registosPrecoRepository = registosPrecoRepository;
            _produtoRepository = produtoRepository;
            _exportService = exportService;
            _context = context;
        }

        [HttpGet("lojas")]
        public async Task<ActionResult<IEnumerable<LojaReportDto>>> GetLojasReport()
        {
            Console.WriteLine("[DEBUG] Iniciando GetLojasReport...");
            try
            {
                var lojas = await _lojaRepository.GetAllWithDetailsAsync();
                var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();
                var report = new List<LojaReportDto>();

                foreach (var loja in lojas)
                {
                    var produtosInfo = registos
                        .Where(r => r.LojaId == loja.LojaId)
                        .GroupBy(r => r.ProdutoId)
                        .Select(g => new ProdutoPriceDto
                        {
                            ProdutoId = g.Key,
                            ProdutoNome = g.Select(r => r.Produto != null ? r.Produto.Nome : "N/A").FirstOrDefault(),
                            LatestPrice = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.Preco ?? 0,
                            LatestDate = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.DataRegisto ?? DateTime.MinValue
                        })
                        .ToList();

                    var categoriaCounts = registos
                        .Where(r => r.LojaId == loja.LojaId)
                        .GroupBy(r => r.Produto?.CategoriaId)
                        .Select(g => new CategoriaCountDto
                        {
                            CategoriaId = g.Key ?? 0,
                            CategoriaNome = g.Select(r => r.Produto != null && r.Produto.Categoria != null ? r.Produto.Categoria.Nome : "N/A").FirstOrDefault(),
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
                Console.WriteLine($"[DEBUG] Relatório de lojas gerado com sucesso: {report.Count} lojas.");
                return Ok(report);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao gerar relatório de lojas: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Message = "Erro ao gerar relatório de lojas." });
            }
        }

        [HttpGet("lojas/{lojaId:int}")]
        public async Task<ActionResult<LojaReportDto>> GetLojaReport(int lojaId)
        {
            Console.WriteLine($"[DEBUG] Iniciando GetLojaReport para LojaId: {lojaId}");
            try
            {
                var loja = await _lojaRepository.GetByIdWithDetailsAsync(lojaId);
                if (loja == null)
                {
                    Console.WriteLine($"[DEBUG] Loja com ID {lojaId} não encontrada.");
                    return NotFound($"Loja com ID {lojaId} não encontrada.");
                }

                var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();

                var produtosInfo = registos
                    .Where(r => r.LojaId == lojaId)
                    .GroupBy(r => r.ProdutoId)
                    .Select(g => new ProdutoPriceDto
                    {
                        ProdutoId = g.Key,
                        ProdutoNome = g.Select(r => r.Produto != null ? r.Produto.Nome : "N/A").FirstOrDefault(),
                        LatestPrice = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.Preco ?? 0,
                        LatestDate = g.OrderByDescending(r => r.DataRegisto).FirstOrDefault()?.DataRegisto ?? DateTime.MinValue
                    })
                    .ToList();

                var categoriaCounts = registos
                    .Where(r => r.LojaId == lojaId)
                    .GroupBy(r => r.Produto?.CategoriaId)
                    .Select(g => new CategoriaCountDto
                    {
                        CategoriaId = g.Key ?? 0,
                        CategoriaNome = g.Select(r => r.Produto != null && r.Produto.Categoria != null ? r.Produto.Categoria.Nome : "N/A").FirstOrDefault(),
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

                Console.WriteLine($"[DEBUG] Relatório para LojaId {lojaId} gerado com sucesso.");
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao gerar relatório para LojaId {lojaId}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Message = "Erro ao gerar relatório da loja." });
            }
        }

        [HttpGet("produtos/{produtoId:int}")]
        public async Task<ActionResult<ProdutoReportDto>> GetProdutoReport(int produtoId)
        {
            Console.WriteLine($"[DEBUG] Iniciando GetProdutoReport para ProdutoId: {produtoId}");
            try
            {
                var produto = await _produtoRepository.GetByIdWithDetailsAsync(produtoId);
                if (produto == null)
                {
                    Console.WriteLine($"[DEBUG] Produto com ID {produtoId} não encontrado.");
                    return NotFound($"Produto com ID {produtoId} não encontrado.");
                }

                var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();

                var lojasInfo = registos
                    .Where(r => r.ProdutoId == produtoId)
                    .GroupBy(r => r.LojaId)
                    .Select(g => new LojaPriceDto
                    {
                        LojaId = g.Key,
                        LojaNome = g.Select(r => r.Loja != null ? r.Loja.Nome : "N/A").FirstOrDefault(),
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

                Console.WriteLine($"[DEBUG] Relatório para ProdutoId {produtoId} gerado com sucesso.");
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao gerar relatório para ProdutoId {produtoId}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Message = "Erro ao gerar relatório do produto." });
            }
        }

        [HttpGet("export")]
        public IActionResult ExportReport(string format)
        {
            Console.WriteLine($"[DEBUG] Iniciando ExportReport com formato: {format}");
            try
            {
                var data = _context.RegistosPrecos
                    .Include(rp => rp.Produto)
                        .ThenInclude(p => p.Categoria!)
                    .Include(rp => rp.Loja)
                    .Select(rp => new Relatorio
                    {
                        NomeProduto = rp.Produto != null ? rp.Produto.Nome : "N/A",
                        ProdutoId = rp.Produto != null ? rp.Produto.ProdutoId : 0,
                        NomeLoja = rp.Loja != null ? rp.Loja.Nome : "N/A",
                        LojaId = rp.Loja != null ? rp.Loja.LojaId : 0,
                        Preco = rp.Preco,
                        Data = rp.DataRegisto,
                        CategoriaId = rp.Produto != null && rp.Produto.Categoria != null ? rp.Produto.Categoria.CategoriaId : 0
                    })
                    .ToList();

                var fileContent = _exportService.ExportReport(data, format);
                var contentType = format.ToLower() == "csv" ? "text/csv" : "application/pdf";
                var fileName = $"relatorio_precos_{DateTime.Now:yyyyMMdd}.{format}";
                Console.WriteLine($"[DEBUG] Relatório exportado com sucesso: {fileName}");
                return File(fileContent, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[ERROR] Erro de argumento ao exportar relatório: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao exportar relatório: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { Message = "Erro ao exportar relatório." });
            }
        }

        [HttpGet("produtos/{produtoId}/pricehistory")]
        public async Task<IActionResult> GetPriceHistory(int produtoId)
        {
            Console.WriteLine($"[DEBUG] Buscando histórico de preços para ProdutoId: {produtoId}");
            try
            {
                // Verificar se o produto existe
                var produtoExists = await _context.Produtos.AnyAsync(p => p.ProdutoId == produtoId);
                if (!produtoExists)
                {
                    Console.WriteLine($"[DEBUG] Produto com ID {produtoId} não encontrado.");
                    return NotFound(new ApiResponse<List<PriceHistoryDto>>
                    {
                        Success = false,
                        Message = $"Produto com ID {produtoId} não encontrado.",
                        StatusCode = 404
                    });
                }

                Console.WriteLine("[DEBUG] Executando query para buscar registros de preços...");
                var registos = await _context.RegistosPrecos
                    .Where(r => r.ProdutoId == produtoId)
                    .Select(r => new
                    {
                        DataRegisto = r.DataRegisto,
                        Preco = r.Preco
                    })
                    .ToListAsync();

                Console.WriteLine($"[DEBUG] Total de registros encontrados: {registos.Count}");

                if (!registos.Any())
                {
                    Console.WriteLine($"[DEBUG] Nenhum registro de preço encontrado para ProdutoId: {produtoId}");
                    return Ok(new ApiResponse<List<PriceHistoryDto>>
                    {
                        Success = true,
                        Data = new List<PriceHistoryDto>(),
                        StatusCode = 200
                    });
                }

                // Agrupar por data e calcular a média dos preços
                var priceHistory = registos
                    .GroupBy(r => r.DataRegisto.Date)
                    .Select(g => new PriceHistoryDto
                    {
                        LojaNome = "Média",
                        Prices = new List<PricePoint>
                        {
                            new PricePoint
                            {
                                Date = g.Key,
                                Price = g.Average(r => r.Preco)
                            }
                        }
                    })
                    .OrderBy(h => h.Prices.First().Date)
                    .ToList();

                Console.WriteLine($"[DEBUG] Histórico de preços encontrado: {priceHistory.Count} datas");
                return Ok(new ApiResponse<List<PriceHistoryDto>>
                {
                    Success = true,
                    Data = priceHistory,
                    StatusCode = 200
                });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"[ERROR] Erro no banco ao buscar histórico de preços para ProdutoId {produtoId}: {dbEx.Message}\n{dbEx.InnerException?.Message}\n{dbEx.StackTrace}");
                return StatusCode(500, new ApiResponse<List<PriceHistoryDto>>
                {
                    Success = false,
                    Message = "Erro ao buscar o histórico de preços devido a um problema no banco de dados.",
                    StatusCode = 500
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao buscar histórico de preços para ProdutoId {produtoId}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new ApiResponse<List<PriceHistoryDto>>
                {
                    Success = false,
                    Message = "Erro ao buscar o histórico de preços.",
                    StatusCode = 500
                });
            }
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

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }
    }
}
