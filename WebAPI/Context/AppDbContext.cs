using Microsoft.EntityFrameworkCore;
using WebAPI.Entities;
using WebAPI.Helpers;

namespace WebAPI.Context
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Categoria> Categorias { get; set; }
        public virtual DbSet<Localizacao> Localizacaos { get; set; }
        public virtual DbSet<Loja> Lojas { get; set; }
        public virtual DbSet<Produto> Produtos { get; set; }
        public virtual DbSet<RegistosPreco> RegistosPrecos { get; set; }
        public virtual DbSet<TipoAcao> TipoAcaos { get; set; }
        public virtual DbSet<TipoUtilizador> TipoUtilizadors { get; set; }
        public virtual DbSet<Utilizador> Utilizadores { get; set; }
        public virtual DbSet<Mensagem> Mensagens { get; set; }
        public virtual DbSet<Relatorio> Relatorios { get; set; }
        public virtual DbSet<Comentario> Comentarios { get; set; }
        public virtual DbSet<Favorito> Favoritos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code.
                optionsBuilder.UseNpgsql("Host=localhost;Database=ES2;Username=postgres;Password=batata");
            }
        }

        public void SeedInitialData()
        {
            try
            {
                // Verifica e cria os TipoUtilizador se não existirem
                string[] requiredTypes = { "ADMIN", "USER", "USER_MANAGER" };
                foreach (var type in requiredTypes)
                {
                    if (!TipoUtilizadors.Any(t => t.Tipo == type))
                    {
                        TipoUtilizadors.Add(new TipoUtilizador { Tipo = type });
                    }
                }

                // Garante que os TipoUtilizador sejam salvos antes de criar o utilizador Admin
                if (requiredTypes.Any(type => !TipoUtilizadors.Any(t => t.Tipo == type)))
                {
                    SaveChanges();
                    Console.WriteLine("[DEBUG] Tipos de Utilizador criados ou encontrados com sucesso.");
                }

                // Verifica se já existe um utilizador Admin
                if (!Utilizadores.Any(u => u.TipoUtilizador.Tipo == "ADMIN"))
                {
                    var adminType = TipoUtilizadors.First(t => t.Tipo == "ADMIN");
                    var adminPasswordHash = PasswordHelper.HashPassword("admin123");
                    Utilizadores.Add(new Utilizador
                    {
                        UtilizadorId = Utilizadores.Any() ? Utilizadores.Max(u => u.UtilizadorId) + 1 : 1,
                        Username = "admin",
                        Email = "admin@example.com",
                        Password = adminPasswordHash,
                        TipoUtilizadorId = adminType.TipoUtilizadorId,
                        DataCriacao = DateTime.UtcNow
                    });
                    SaveChanges();
                    Console.WriteLine("[DEBUG] Utilizador Admin criado com sucesso: admin/admin123");
                }
                else
                {
                    Console.WriteLine("[DEBUG] Já existe um utilizador Admin. Nenhum novo Admin foi criado.");
                }

                // Verifica se há mais TipoUtilizador do que o esperado
                var existingTypes = TipoUtilizadors.ToList();
                if (existingTypes.Count > requiredTypes.Length)
                {
                    Console.WriteLine($"[WARNING] Existem {existingTypes.Count} tipos de utilizador, mas apenas {requiredTypes.Length} são esperados: {string.Join(", ", requiredTypes)}.");
                    Console.WriteLine($"[WARNING] Tipos atuais: {string.Join(", ", existingTypes.Select(t => $"{t.Tipo} (ID: {t.TipoUtilizadorId})"))}.");
                    Console.WriteLine("[WARNING] Considere limpar os tipos duplicados manualmente no PostgreSQL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao realizar o seeding inicial: {ex.Message}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.CategoriaId).HasName("Categorias_pkey");
                entity.ToTable("Categorias");
                entity.Property(e => e.Nome).HasMaxLength(100);
                entity.HasOne(c => c.Parent)
                    .WithMany(c => c.SubCategorias)
                    .HasForeignKey(c => c.ParentId)
                    .HasConstraintName("FK_Categorias_Parent");
            });

            modelBuilder.Entity<Localizacao>(entity =>
            {
                entity.HasKey(e => e.LocalizacaoId).HasName("Localizacao_pkey");
                entity.ToTable("Localizacao");
                entity.Property(e => e.Cidade).HasMaxLength(100);
                entity.Property(e => e.CodigoPostal).HasMaxLength(20);
                entity.Property(e => e.GoogleMapsUrl).HasMaxLength(300);
                entity.Property(e => e.Latitude).HasPrecision(10, 6);
                entity.Property(e => e.Longitude).HasPrecision(10, 6);
                entity.Property(e => e.Pais).HasMaxLength(100);
            });

            modelBuilder.Entity<Loja>(entity =>
            {
                entity.HasKey(e => e.LojaId).HasName("Lojas_pkey");
                entity.ToTable("Lojas");
                entity.Property(e => e.Endereco).HasMaxLength(200);
                entity.Property(e => e.Nome).HasMaxLength(100);
                entity.HasOne(d => d.Localizacao).WithMany(p => p.Lojas)
                    .HasForeignKey(d => d.LocalizacaoId)
                    .HasConstraintName("FK_Lojas_Localizacao");
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(e => e.ProdutoId).HasName("Produtos_pkey");
                entity.ToTable("Produtos");
                entity.Property(e => e.Descricao).HasMaxLength(200);
                entity.Property(e => e.Marca).HasMaxLength(100);
                entity.Property(e => e.Nome).HasMaxLength(100);
                entity.HasOne(d => d.Categoria).WithMany(p => p.Produtos)
                    .HasForeignKey(d => d.CategoriaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Produtos_Categorias");
            });

            modelBuilder.Entity<RegistosPreco>(entity =>
            {
                entity.HasKey(e => e.RegistoPrecoId).HasName("RegistosPrecos_pkey");
                entity.ToTable("RegistosPrecos");
                entity.Property(e => e.Credibilidade).HasPrecision(5, 2);
                entity.Property(e => e.DataRegisto).HasColumnType("timestamp without time zone");
                entity.Property(e => e.Preco).HasPrecision(10, 2);
                entity.HasOne(d => d.Loja).WithMany(p => p.RegistosPrecos)
                    .HasForeignKey(d => d.LojaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegistosPrecos_Lojas");
                entity.HasOne(d => d.Produto).WithMany(p => p.RegistosPrecos)
                    .HasForeignKey(d => d.ProdutoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegistosPrecos_Produtos");
                entity.HasOne(d => d.TipoAcao).WithMany(p => p.RegistosPrecos)
                    .HasForeignKey(d => d.TipoAcaoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegistosPrecos_TipoAcao");
                entity.HasOne(d => d.Utilizador).WithMany(p => p.RegistosPrecos)
                    .HasForeignKey(d => d.UtilizadorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegistosPrecos_Utilizadores");
            });

            modelBuilder.Entity<TipoAcao>(entity =>
            {
                entity.HasKey(e => e.TipoAcaoId).HasName("TipoAcao_pkey");
                entity.ToTable("TipoAcao");
                entity.Property(e => e.Tipo).HasMaxLength(50);
            });

            modelBuilder.Entity<TipoUtilizador>(entity =>
            {
                entity.HasKey(e => e.TipoUtilizadorId).HasName("TipoUtilizador_pkey");
                entity.ToTable("TipoUtilizador");
                entity.Property(e => e.Tipo).HasMaxLength(50);
            });

            modelBuilder.Entity<Utilizador>(entity =>
            {
                entity.HasKey(e => e.UtilizadorId).HasName("Utilizadores_pkey");
                entity.ToTable("Utilizadores");
                entity.Property(e => e.Cargo).HasMaxLength(50);
                entity.Property(e => e.DataCriacao);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.GoogleToken).HasMaxLength(200);
                entity.Property(e => e.Password).HasMaxLength(200);
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.GoogleId).HasMaxLength(50);
                entity.HasOne(d => d.TipoUtilizador).WithMany(p => p.Utilizadores)
                    .HasForeignKey(d => d.TipoUtilizadorId)
                    .HasConstraintName("FK_Utilizadores_TipoUtilizador");
            });

            modelBuilder.Entity<Mensagem>(entity =>
            {
                entity.HasKey(e => e.MensagemId).HasName("Mensagens_pkey");
                entity.ToTable("Mensagens");
                entity.Property(e => e.Conteudo).HasMaxLength(1000);
                entity.Property(e => e.DataEnvio).HasColumnType("timestamp with time zone");
                entity.HasOne(d => d.Remetente).WithMany(p => p.MensagensEnviadas)
                    .HasForeignKey(d => d.RemetenteId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Mensagens_Utilizadores_RemetenteId");
                entity.HasOne(d => d.Destinatario).WithMany(p => p.MensagensRecebidas)
                    .HasForeignKey(d => d.DestinatarioId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Mensagens_Utilizadores_DestinatarioId");
            });

            modelBuilder.Entity<Relatorio>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("Relatorios_pkey");
                entity.ToTable("Relatorios");
                entity.Property(e => e.NomeProduto).HasMaxLength(255);
                entity.Property(e => e.NomeLoja).HasMaxLength(255);
                entity.Property(e => e.Preco).HasPrecision(18, 2);
                entity.Property(e => e.Data).HasColumnType("timestamp without time zone");
                entity.Property(e => e.NomeCategoria).HasMaxLength(255);
                entity.HasOne(d => d.Produto).WithMany()
                    .HasForeignKey(d => d.ProdutoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Relatorios_Produtos");
                entity.HasOne(d => d.Loja).WithMany()
                    .HasForeignKey(d => d.LojaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Relatorios_Lojas");
                entity.HasOne(d => d.Categoria).WithMany()
                    .HasForeignKey(d => d.CategoriaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Relatorios_Categorias");
            });

            modelBuilder.Entity<Comentario>(entity =>
            {
                entity.HasKey(e => e.ComentarioId).HasName("Comentarios_pkey");
                entity.ToTable("Comentarios");
                entity.Property(e => e.Conteudo).HasMaxLength(1000);
                entity.Property(e => e.DataCriacao).HasColumnType("timestamp with time zone");
                entity.HasOne(c => c.RegistoPreco)
                    .WithMany(r => r.Comentarios)
                    .HasForeignKey(c => c.RegistoPrecoId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Comentarios_RegistosPrecos");
                entity.HasOne(c => c.Utilizador)
                    .WithMany()
                    .HasForeignKey(c => c.UtilizadorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Comentarios_Utilizadores");
            });

            modelBuilder.Entity<Favorito>(entity =>
            {
                entity.HasKey(e => e.FavoritoId).HasName("Favoritos_pkey");
                entity.ToTable("Favoritos");
                entity.HasOne(f => f.Utilizador)
                    .WithMany(u => u.Favoritos)
                    .HasForeignKey(f => f.UtilizadorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Favoritos_Utilizadores");
                entity.HasOne(f => f.Produto)
                    .WithMany(p => p.Favoritos)
                    .HasForeignKey(f => f.ProdutoId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Favoritos_Produtos");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
