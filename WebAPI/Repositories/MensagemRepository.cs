using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class MensagemRepository : Repository<Mensagem>, IMensagemRepository
    {
        public MensagemRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Mensagem>> GetAllWithDetailsAsync()
        {
            return await DbSet
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();
        }

        public async Task<IEnumerable<Mensagem>> GetByUserIdAsync(int userId)
        {
            return await DbSet
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .Where(m => m.RemetenteId == userId || m.DestinatarioId == userId)
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();
        }
    }
}
