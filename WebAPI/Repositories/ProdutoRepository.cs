using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class ProdutoRepository : Repository<Produto>, IProdutoRepository
    {
        public ProdutoRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Produto>> GetAllWithDetailsAsync()
        {
            var produtos = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.RegistosPrecos)
                    .ThenInclude(rp => rp.Loja)
                .ToListAsync();
            Console.WriteLine($"[DEBUG] GetAllWithDetailsAsync retornou {produtos.Count} produtos.");
            return produtos;
        }

        public async Task<Produto> GetByIdWithDetailsAsync(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.RegistosPrecos)
                    .ThenInclude(rp => rp.Loja)
                .FirstOrDefaultAsync(p => p.ProdutoId == id)
                ?? throw new KeyNotFoundException($"Produto com ID {id} não encontrado.");
            Console.WriteLine($"[DEBUG] GetByIdWithDetailsAsync({id}) retornou: {(produto != null ? produto.Nome : "null")}");
            return produto;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var exists = await _context.Produtos.AnyAsync(p => p.ProdutoId == id);
            Console.WriteLine($"[DEBUG] ExistsAsync({id}) retornou: {exists}");
            return exists;
        }

        public async Task<IEnumerable<Produto>> FindWithDetailsAsync(Expression<Func<Produto, bool>> predicate)
        {
            var produtos = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.RegistosPrecos)
                    .ThenInclude(rp => rp.Loja)
                .Where(predicate)
                .ToListAsync();
            Console.WriteLine($"[DEBUG] FindWithDetailsAsync retornou {produtos.Count} produtos.");
            return produtos;
        }
    }
}
