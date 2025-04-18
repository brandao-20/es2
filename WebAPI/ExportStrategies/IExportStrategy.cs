using WebAPI.Entities;

namespace WebAPI.ExportStrategies
{
    public interface IExportStrategy
    {
        byte[] Export(List<Relatorio> data);
    }
}
