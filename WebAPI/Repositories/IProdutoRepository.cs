using System.Linq.Expressions;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface IProdutoRepository : IRepository<Produto>
    {
        Task<IEnumerable<Produto>> GetAllWithDetailsAsync();
        Task<Produto> GetByIdWithDetailsAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Produto>> FindWithDetailsAsync(Expression<Func<Produto, bool>> predicate);
    }
}
