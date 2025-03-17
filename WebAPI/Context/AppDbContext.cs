using System;
using System.Collections.Generic;
using ES2_TP_ComparadorPrecos_WebAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ES2_TP_ComparadorPrecos_WebAPI.Context;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=ES2;Username=postgres;Password=batata");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("Categorias_pkey");

            entity.Property(e => e.Nome).HasMaxLength(100);
        });

        modelBuilder.Entity<Localizacao>(entity =>
        {
            entity.HasKey(e => e.LocalizacaoId).HasName("Localizacao_pkey");

            entity.ToTable("Localizacao");

            entity.Property(e => e.Cidade).HasMaxLength(100);
            entity.Property(e => e.CodigoPostal).HasMaxLength(20);
            entity.Property(e => e.GoogleMapsUrl).HasMaxLength(300);
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.Pais).HasMaxLength(100);
        });

        modelBuilder.Entity<Loja>(entity =>
        {
            entity.HasKey(e => e.LojaId).HasName("Lojas_pkey");

            entity.Property(e => e.Endereco).HasMaxLength(200);
            entity.Property(e => e.Nome).HasMaxLength(100);

            entity.HasOne(d => d.Localizacao).WithMany(p => p.Lojas)
                .HasForeignKey(d => d.LocalizacaoId)
                .HasConstraintName("FK_Lojas_Localizacao");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.ProdutoId).HasName("Produtos_pkey");

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

            entity.Property(e => e.Cargo).HasMaxLength(50);
            entity.Property(e => e.DataCriacao)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GoogleToken).HasMaxLength(200);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Telefone).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.TipoUtilizador).WithMany(p => p.Utilizadores)
                .HasForeignKey(d => d.TipoUtilizadorId)
                .HasConstraintName("FK_Utilizadores_TipoUtilizador");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
