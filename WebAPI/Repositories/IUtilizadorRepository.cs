using System.Linq.Expressions;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface IUtilizadorRepository : IRepository<Utilizador>
    {
        Task<IEnumerable<Utilizador>> GetAllWithDetailsAsync();
        Task<Utilizador> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Utilizador>> FindAsync(Expression<Func<Utilizador, bool>> predicate);
        Task<IEnumerable<Utilizador>> FindWithDetailsAsync(Expression<Func<Utilizador, bool>> predicate);

        // Métodos para paginação:
        Task<int> CountAsync();
        Task<List<Utilizador>> GetPagedWithDetailsAsync(int skip, int take);
    }
}
