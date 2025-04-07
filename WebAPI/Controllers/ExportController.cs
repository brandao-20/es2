using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly ILojaRepository _lojaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IRegistosPrecoRepository _registosPrecoRepository;

        public ExportController(
            ILojaRepository lojaRepository,
            IProdutoRepository produtoRepository,
            IRegistosPrecoRepository registosPrecoRepository)
        {
            _lojaRepository = lojaRepository;
            _produtoRepository = produtoRepository;
            _registosPrecoRepository = registosPrecoRepository;
        }

        // Exporta Relatório Geral de Lojas em CSV
        [HttpGet("lojas/csv")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> ExportLojasCsv()
        {
            var lojas = await _lojaRepository.GetAllWithDetailsAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Loja;Endereco;Cidade;Pais");

            foreach (var loja in lojas)
            {
                var cidade = loja.Localizacao?.Cidade ?? "";
                var pais = loja.Localizacao?.Pais ?? "";
                sb.AppendLine($"{loja.Nome};{loja.Endereco};{cidade};{pais}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", "relatorio_lojas.csv");
        }

        // Exporta Relatório Geral de Lojas em PDF
        [HttpGet("lojas/pdf")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> ExportLojasPdf()
        {
            var lojas = await _lojaRepository.GetAllWithDetailsAsync();

            using var memoryStream = new MemoryStream();
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
                document.Add(new Paragraph($"Endereço: {loja.Endereco}"));
                document.Add(new Paragraph($"Localização: {loja.Localizacao?.Cidade ?? "N/A"}, {loja.Localizacao?.Pais ?? "N/A"}"));
                document.Add(new Paragraph(" "));
            }

            document.Close();
            var pdfBytes = memoryStream.ToArray();
            return File(pdfBytes, "application/pdf", "relatorio_lojas.pdf");
        }

        // Exporta Relatório Geral de Produtos em PDF
        [HttpGet("produtos/pdf")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> ExportProdutosPdf()
        {
            var produtos = await _produtoRepository.GetAllWithDetailsAsync();

            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            document.Add(new Paragraph("Relatório de Produtos")
                .SetFontSize(16)
                .SetBold());
            document.Add(new Paragraph(" "));

            foreach (var p in produtos)
            {
                document.Add(new Paragraph($"Produto: {p.Nome}").SetBold());
                document.Add(new Paragraph($"Marca: {p.Marca}"));
                document.Add(new Paragraph($"Categoria: {p.Categoria?.Nome ?? "N/A"}"));
                document.Add(new Paragraph(" "));
            }

            document.Close();
            var pdfBytes = memoryStream.ToArray();
            return File(pdfBytes, "application/pdf", "relatorio_produtos.pdf");
        }
    }
}
