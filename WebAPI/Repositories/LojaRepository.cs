using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class LojaRepository : Repository<Loja>, ILojaRepository
    {
        public LojaRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Loja>> GetAllWithDetailsAsync()
        {
            return await DbSet
                .Include(l => l.Localizacao)
                .ToListAsync();
        }

        public async Task<Loja> GetByIdWithDetailsAsync(int id)
        {
            return await DbSet
                .Include(l => l.Localizacao)
                .FirstOrDefaultAsync(l => l.LojaId == id)
                ?? throw new KeyNotFoundException($"Loja with ID {id} not found.");
        }
    }
}
