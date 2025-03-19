using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ExportController(AppDbContext context)
        {
            _context = context;
        }

        // Exporta relatório de lojas em CSV
        [HttpGet("lojas/csv")]
        public async Task<IActionResult> ExportLojasCsv()
        {
            var lojas = await _context.Lojas.Include(l => l.Localizacao).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("LojaId,Nome,Endereco,Cidade,Pais");
            foreach (var loja in lojas)
            {
                var cidade = loja.Localizacao?.Cidade ?? "";
                var pais = loja.Localizacao?.Pais ?? "";
                sb.AppendLine($"{loja.LojaId},{loja.Nome},{loja.Endereco},{cidade},{pais}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "lojas_report.csv");
        }

        // Exporta relatório de lojas em PDF (exemplo simples usando PdfSharp – requer a biblioteca PdfSharpCore)
        [HttpGet("lojas/pdf")]
        public async Task<IActionResult> ExportLojasPdf()
        {
            var lojas = await _context.Lojas.Include(l => l.Localizacao).ToListAsync();

            // Aqui, para fins de exemplo, geramos um PDF simples.
            // Em um cenário real, você utilizaria uma biblioteca como PdfSharpCore ou QuestPDF.
            using var ms = new MemoryStream();
            // [Pseudo-código:] Gerar PDF com lojas e suas informações
            // Por exemplo, usando QuestPDF:
            // var document = Document.Create(container => { ... });
            // document.GeneratePdf(ms);

            // Para este exemplo, retornaremos um PDF vazio.
            byte[] pdfBytes = ms.ToArray();
            return File(pdfBytes, "application/pdf", "lojas_report.pdf");
        }

        // Endpoints para XLSX podem ser implementados de forma similar, utilizando ClosedXML ou EPPlus.
    }
}
