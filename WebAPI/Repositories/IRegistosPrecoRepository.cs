using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface IRegistosPrecoRepository : IRepository<RegistosPreco>
    {
        Task<IEnumerable<RegistosPreco>> GetAllWithDetailsAsync();
        Task<RegistosPreco> GetByIdWithDetailsAsync(int id);
    }
}
