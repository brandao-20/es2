using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class UtilizadorRepository : Repository<Utilizador>, IUtilizadorRepository
    {
        public UtilizadorRepository(AppDbContext context) : base(context) { }

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

        public async Task<IEnumerable<Utilizador>> FindAsync(Expression<Func<Utilizador, bool>> predicate)
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
    }
}
