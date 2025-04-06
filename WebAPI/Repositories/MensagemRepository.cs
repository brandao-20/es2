using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class MensagemRepository : Repository<Mensagem>, IMensagemRepository
    {

        public MensagemRepository(AppDbContext context) : base(context) { }
        
        public async Task<List<Mensagem>> GetByUserIdAsync(int userId)
        {
            return await _context.Mensagens
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .Where(m => m.RemetenteId == userId || m.DestinatarioId == userId)
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();
        }

        public async Task<Mensagem> GetByIdAsync(int id)
        {
            return await _context.Mensagens
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .FirstOrDefaultAsync(m => m.MensagemId == id)
                ?? throw new KeyNotFoundException($"Mensagem com ID {id} não encontrada.");
        }

        public async Task<List<Mensagem>> GetAllWithDetailsAsync()
        {
            return await _context.Mensagens
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .ToListAsync();
        }

        public async Task AddAsync(Mensagem mensagem)
        {
            await _context.Mensagens.AddAsync(mensagem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Mensagem mensagem)
        {
            _context.Mensagens.Remove(mensagem);
            await _context.SaveChangesAsync();
        }
    }
}
