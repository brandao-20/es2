using WebAPI.Entities;

namespace WebAPI.ExportStrategies
{
    public class ReportExportService
    {
        private readonly Dictionary<string, IExportStrategy> _strategies;

        public ReportExportService()
        {
            _strategies = new Dictionary<string, IExportStrategy>
            {
                { "csv", new CsvExportStrategy() },
                { "pdf", new PdfExportStrategy() }
            };
        }

        public byte[] ExportReport(List<Relatorio> data, string format)
        {
            if (!_strategies.TryGetValue(format.ToLower(), out var strategy))
            {
                throw new ArgumentException("Formato de exportação não suportado.");
            }
            return strategy.Export(data);
        }
    }
}
