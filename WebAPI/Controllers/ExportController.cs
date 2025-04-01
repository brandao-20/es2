using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IRegistosPrecoRepository _registosPrecoRepository;
        private readonly ILojaRepository _lojaRepository;
        private readonly IProdutoRepository _produtoRepository;

        public ExportController(
            IRegistosPrecoRepository registosPrecoRepository,
            ILojaRepository lojaRepository,
            IProdutoRepository produtoRepository)
        {
            _registosPrecoRepository = registosPrecoRepository;
            _lojaRepository = lojaRepository;
            _produtoRepository = produtoRepository;
        }

        [HttpGet("stores-report")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> ExportStoresReport()
        {
            var lojas = await _lojaRepository.GetAllWithDetailsAsync();
            var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph("Relatório de Lojas")
                    .SetFontSize(16)
                    .SetBold());
                document.Add(new Paragraph(" "));

                foreach (var loja in lojas)
                {
                    document.Add(new Paragraph($"Loja: {loja.Nome}")
                        .SetFontSize(12)
                        .SetBold());
                    document.Add(new Paragraph($"Localização: {loja.Localizacao?.Cidade ?? "N/A"}, {loja.Localizacao?.Pais ?? "N/A"}"));
                    document.Add(new Paragraph("Produtos:"));

                    var lojaRegistos = registos.Where(r => r.LojaId == loja.LojaId).ToList();
                    if (lojaRegistos.Any())
                    {
                        var table = new Table(UnitValue.CreatePercentArray(new float[] { 25, 25, 25, 25 }));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Produto").SetBold()));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Preço").SetBold()));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Data").SetBold()));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Credibilidade").SetBold()));

                        foreach (var registo in lojaRegistos)
                        {
                            table.AddCell(new Cell().Add(new Paragraph(registo.Produto?.Nome ?? "N/A")));
                            table.AddCell(new Cell().Add(new Paragraph(registo.Preco.ToString("C"))));
                            table.AddCell(new Cell().Add(new Paragraph(registo.DataRegisto.ToString("dd/MM/yyyy"))));
                            table.AddCell(new Cell().Add(new Paragraph(registo.Credibilidade.ToString())));
                        }

                        document.Add(table);
                    }
                    else
                    {
                        document.Add(new Paragraph("Nenhum produto registrado."));
                    }

                    document.Add(new Paragraph(" "));
                }

                document.Close();
                var bytes = memoryStream.ToArray();
                return File(bytes, "application/pdf", "relatorio_lojas.pdf");
            }
        }

        [HttpGet("products-report")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> ExportProductsReport()
        {
            var produtos = await _produtoRepository.GetAllWithDetailsAsync();
            var registos = await _registosPrecoRepository.GetAllWithDetailsAsync();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph("Relatório de Produtos")
                    .SetFontSize(16)
                    .SetBold());
                document.Add(new Paragraph(" "));

                foreach (var produto in produtos)
                {
                    document.Add(new Paragraph($"Produto: {produto.Nome} ({produto.Marca ?? "N/A"})")
                        .SetFontSize(12)
                        .SetBold());
                    document.Add(new Paragraph($"Categoria: {produto.Categoria?.Nome ?? "N/A"}"));
                    document.Add(new Paragraph("Preços:"));

                    var produtoRegistos = registos.Where(r => r.ProdutoId == produto.ProdutoId).ToList();
                    if (produtoRegistos.Any())
                    {
                        var table = new Table(UnitValue.CreatePercentArray(new float[] { 25, 25, 25, 25 }));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Loja").SetBold()));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Preço").SetBold()));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Data").SetBold()));
                        table.AddHeaderCell(new Cell().Add(new Paragraph("Credibilidade").SetBold()));

                        foreach (var registo in produtoRegistos)
                        {
                            table.AddCell(new Cell().Add(new Paragraph(registo.Loja?.Nome ?? "N/A")));
                            table.AddCell(new Cell().Add(new Paragraph(registo.Preco.ToString("C"))));
                            table.AddCell(new Cell().Add(new Paragraph(registo.DataRegisto.ToString("dd/MM/yyyy"))));
                            table.AddCell(new Cell().Add(new Paragraph(registo.Credibilidade.ToString())));
                        }

                        document.Add(table);
                    }
                    else
                    {
                        document.Add(new Paragraph("Nenhum preço registrado."));
                    }

                    document.Add(new Paragraph(" "));
                }

                document.Close();
                var bytes = memoryStream.ToArray();
                return File(bytes, "application/pdf", "relatorio_produtos.pdf");
            }
        }
    }
}
