using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    CategoriaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Categorias_pkey", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "Localizacao",
                columns: table => new
                {
                    LocalizacaoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Pais = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CodigoPostal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    GoogleMapsUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Localizacao_pkey", x => x.LocalizacaoId);
                });

            migrationBuilder.CreateTable(
                name: "TipoAcao",
                columns: table => new
                {
                    TipoAcaoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TipoAcao_pkey", x => x.TipoAcaoId);
                });

            migrationBuilder.CreateTable(
                name: "TipoUtilizador",
                columns: table => new
                {
                    TipoUtilizadorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TipoUtilizador_pkey", x => x.TipoUtilizadorId);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    ProdutoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Produtos_pkey", x => x.ProdutoId);
                    table.ForeignKey(
                        name: "FK_Produtos_Categorias",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "CategoriaId");
                });

            migrationBuilder.CreateTable(
                name: "Lojas",
                columns: table => new
                {
                    LojaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Endereco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocalizacaoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Lojas_pkey", x => x.LojaId);
                    table.ForeignKey(
                        name: "FK_Lojas_Localizacao",
                        column: x => x.LocalizacaoId,
                        principalTable: "Localizacao",
                        principalColumn: "LocalizacaoId");
                });

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    UtilizadorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GoogleToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GoogleId = table.Column<string>(type: "text", nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Cargo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoUtilizadorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Utilizadores_pkey", x => x.UtilizadorId);
                    table.ForeignKey(
                        name: "FK_Utilizadores_TipoUtilizador",
                        column: x => x.TipoUtilizadorId,
                        principalTable: "TipoUtilizador",
                        principalColumn: "TipoUtilizadorId");
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    MensagemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RemetenteId = table.Column<int>(type: "integer", nullable: false),
                    DestinatarioId = table.Column<int>(type: "integer", nullable: false),
                    Conteudo = table.Column<string>(type: "text", nullable: true),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.MensagemId);
                    table.ForeignKey(
                        name: "FK_Mensagens_Utilizadores_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Utilizadores",
                        principalColumn: "UtilizadorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mensagens_Utilizadores_RemetenteId",
                        column: x => x.RemetenteId,
                        principalTable: "Utilizadores",
                        principalColumn: "UtilizadorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistosPrecos",
                columns: table => new
                {
                    RegistoPrecoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Credibilidade = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TipoAcaoId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    LojaId = table.Column<int>(type: "integer", nullable: false),
                    UtilizadorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RegistosPrecos_pkey", x => x.RegistoPrecoId);
                    table.ForeignKey(
                        name: "FK_RegistosPrecos_Lojas",
                        column: x => x.LojaId,
                        principalTable: "Lojas",
                        principalColumn: "LojaId");
                    table.ForeignKey(
                        name: "FK_RegistosPrecos_Produtos",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoId");
                    table.ForeignKey(
                        name: "FK_RegistosPrecos_TipoAcao",
                        column: x => x.TipoAcaoId,
                        principalTable: "TipoAcao",
                        principalColumn: "TipoAcaoId");
                    table.ForeignKey(
                        name: "FK_RegistosPrecos_Utilizadores",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "UtilizadorId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lojas_LocalizacaoId",
                table: "Lojas",
                column: "LocalizacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_DestinatarioId",
                table: "Mensagens",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_RemetenteId",
                table: "Mensagens",
                column: "RemetenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_CategoriaId",
                table: "Produtos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistosPrecos_LojaId",
                table: "RegistosPrecos",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistosPrecos_ProdutoId",
                table: "RegistosPrecos",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistosPrecos_TipoAcaoId",
                table: "RegistosPrecos",
                column: "TipoAcaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistosPrecos_UtilizadorId",
                table: "RegistosPrecos",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_TipoUtilizadorId",
                table: "Utilizadores",
                column: "TipoUtilizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "RegistosPrecos");

            migrationBuilder.DropTable(
                name: "Lojas");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "TipoAcao");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropTable(
                name: "Localizacao");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "TipoUtilizador");
        }
    }
}
