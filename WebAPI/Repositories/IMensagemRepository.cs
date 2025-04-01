using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface IMensagemRepository : IRepository<Mensagem>
    {
        Task<IEnumerable<Mensagem>> GetAllWithDetailsAsync();
        Task<IEnumerable<Mensagem>> GetByUserIdAsync(int userId); // Para histórico do usuário
    }
}
