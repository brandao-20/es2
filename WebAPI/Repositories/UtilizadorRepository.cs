using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class UtilizadorRepository : Repository<Utilizador>, IUtilizadorRepository
    {
        private new readonly AppDbContext _context;

        public UtilizadorRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Utilizador>> GetAllWithDetailsAsync()
        {
            return await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .ToListAsync();
        }

        public async Task<Utilizador> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .FirstOrDefaultAsync(u => u.UtilizadorId == id)
                ?? throw new KeyNotFoundException($"Utilizador com ID {id} não encontrado.");
        }

        public new async Task<IEnumerable<Utilizador>> FindAsync(Expression<Func<Utilizador, bool>> predicate)
        {
            return await _context.Utilizadores
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Utilizador>> FindWithDetailsAsync(Expression<Func<Utilizador, bool>> predicate)
        {
            return await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Utilizadores.CountAsync();
        }

        public async Task<List<Utilizador>> GetPagedWithDetailsAsync(int skip, int take)
        {
            return await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .OrderBy(u => u.UtilizadorId)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}
