using Microsoft.EntityFrameworkCore;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class RegistosPrecoRepository : Repository<RegistosPreco>, IRegistosPrecoRepository
    {
        public RegistosPrecoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RegistosPreco>> GetAllWithDetailsAsync()
        {
            return await DbSet
                .Include(r => r.Produto!)
                    .ThenInclude(p => p.Categoria!)
                .Include(r => r.Loja!)
                    .ThenInclude(l => l.Localizacao!)
                .Include(r => r.Utilizador!)
                .Include(r => r.TipoAcao!)
                .ToListAsync();
        }

        public async Task<RegistosPreco> GetByIdWithDetailsAsync(int id)
        {
            return await DbSet
                .Include(r => r.Produto!)
                    .ThenInclude(p => p.Categoria!)
                .Include(r => r.Loja!)
                    .ThenInclude(l => l.Localizacao!)
                .Include(r => r.Utilizador!)
                .Include(r => r.TipoAcao!)
                .FirstOrDefaultAsync(r => r.RegistoPrecoId == id)
                ?? throw new KeyNotFoundException($"RegistoPreco with ID {id} not found.");
        }

        public async Task<RegistosPreco?> GetLatestPriceAsync(int produtoId, int lojaId)
        {
            return await DbSet
                .Where(r => r.ProdutoId == produtoId && r.LojaId == lojaId)
                .OrderByDescending(r => r.DataRegisto)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RegistosPreco>> GetByProdutoIdAsync(int produtoId)
        {
            return await DbSet
                .Where(r => r.ProdutoId == produtoId)
                .Include(r => r.Produto!)
                    .ThenInclude(p => p.Categoria!)
                .Include(r => r.Loja!)
                    .ThenInclude(l => l.Localizacao!)
                .Include(r => r.Utilizador!)
                .Include(r => r.TipoAcao!)
                .ToListAsync();
        }
    }
}
