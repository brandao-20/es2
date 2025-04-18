using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using WebAPI.Entities;

namespace WebAPI.ExportStrategies
{
    public class PdfExportStrategy : IExportStrategy
    {
        public byte[] Export(List<Relatorio> data)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Título
            document.Add(new Paragraph("Relatório de Preços")
                .SetFontSize(16)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            // Tabela
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 30, 30, 20, 20 }));
            table.SetWidth(UnitValue.CreatePercentValue(100));

            // Cabeçalhos
            table.AddHeaderCell("Produto");
            table.AddHeaderCell("Loja");
            table.AddHeaderCell("Preço");
            table.AddHeaderCell("Data");

            // Dados
            foreach (var item in data)
            {
                table.AddCell(item.NomeProduto);
                table.AddCell(item.NomeLoja);
                table.AddCell(item.Preco.ToString("C"));
                table.AddCell(item.Data.ToString("yyyy-MM-dd"));
            }

            document.Add(table);
            document.Close();

            return stream.ToArray();
        }
    }
}
