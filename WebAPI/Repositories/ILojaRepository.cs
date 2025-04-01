using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface ILojaRepository : IRepository<Loja>
    {
        Task<IEnumerable<Loja>> GetAllWithDetailsAsync();
        Task<Loja> GetByIdWithDetailsAsync(int id);
    }
}
