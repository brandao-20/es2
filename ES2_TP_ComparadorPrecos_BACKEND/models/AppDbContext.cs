using Microsoft.EntityFrameworkCore;

namespace ES2_TP_ComparadorPrecos_BACKEND.models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Loja> Lojas { get; set; } = null!;
        public DbSet<RegistoPreco> RegistosPrecos { get; set; } = null!;
        public DbSet<Utilizador> Utilizadores { get; set; } = null!;

        // Caso precises de configurar algo específico:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Fluent API, constraints, etc.
        }
    }
}
