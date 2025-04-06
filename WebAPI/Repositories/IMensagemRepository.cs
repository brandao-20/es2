using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface IMensagemRepository : IRepository<Mensagem>
    {
        Task<List<Mensagem>> GetAllWithDetailsAsync();
        Task<List<Mensagem>> GetByUserIdAsync(int userId); // Para histórico do utilizador
    }
}
