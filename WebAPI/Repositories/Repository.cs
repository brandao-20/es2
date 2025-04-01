using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebAPI.Context;

namespace WebAPI.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public Repository(AppDbContext context)
        {
            _context = context;
        }

        protected DbSet<T> DbSet => _context.Set<T>();

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id)
                ?? throw new KeyNotFoundException($"{typeof(T).Name} with ID {id} not found.");
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await DbSet.FindAsync(id) != null;
        }

        public async Task AddAsync(T entity)
        {
            await DbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            DbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
