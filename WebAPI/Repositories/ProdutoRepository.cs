using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class ProdutoRepository : Repository<Produto>, IProdutoRepository
    {
        private readonly AppDbContext _context;
        public ProdutoRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Produto>> GetAllWithDetailsAsync()
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.RegistosPrecos)
                    .ThenInclude(rp => rp.Loja)
                .ToListAsync();
        }

        public async Task<Produto> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.RegistosPrecos)
                    .ThenInclude(rp => rp.Loja)
                .FirstOrDefaultAsync(p => p.ProdutoId == id)
                ?? throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Produtos.AnyAsync(p => p.ProdutoId == id);
        }

        public async Task<IEnumerable<Produto>> FindWithDetailsAsync(Expression<Func<Produto, bool>> predicate)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.RegistosPrecos).ThenInclude(rp => rp.Loja)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Produtos.CountAsync();
        }

        public async Task<List<Produto>> GetPagedWithDetailsAsync(int skip, int take)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.RegistosPrecos).ThenInclude(rp => rp.Loja)
                .OrderBy(p => p.ProdutoId)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}
