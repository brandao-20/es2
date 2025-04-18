using System.Text;
using WebAPI.Entities;

namespace WebAPI.ExportStrategies
{
    public class CsvExportStrategy : IExportStrategy
    {
        public byte[] Export(List<Relatorio> data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("NomeProduto,NomeLoja,Preco,Data,NomeCategoria");
            foreach (var item in data)
            {
                sb.AppendLine($"{item.NomeProduto},{item.NomeLoja},{item.Preco},{item.Data:yyyy-MM-dd},{item.NomeCategoria}");
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
