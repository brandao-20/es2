using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class LojaRepository : Repository<Loja>, ILojaRepository
    {
        private readonly AppDbContext _context;

        public LojaRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Loja>> GetAllWithDetailsAsync()
        {
            return await _context.Lojas
                .Include(l => l.Localizacao)
                .ToListAsync();
        }

        public async Task<Loja> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Lojas
                .Include(l => l.Localizacao)
                .FirstOrDefaultAsync(l => l.LojaId == id)
                ?? throw new KeyNotFoundException($"Loja with ID {id} not found.");
        }

        public async Task<int> CountAsync()
        {
            return await _context.Lojas.CountAsync();
        }

        public async Task<List<Loja>> GetPagedWithDetailsAsync(int skip, int take)
        {
            return await _context.Lojas
                .Include(l => l.Localizacao)
                .OrderBy(l => l.LojaId)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Loja>> FindWithDetailsAsync(Expression<Func<Loja, bool>> predicate)
        {
            return await _context.Lojas
                .Include(l => l.Localizacao)
                .Where(predicate)
                .ToListAsync();
        }
    }
}
