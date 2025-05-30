using System;
using Microsoft.EntityFrameworkCore.Migrations;
using WebAPI.Helpers;

#nullable disable

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GoogleId",
                table: "Utilizadores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Conteudo",
                table: "Mensagens",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Localizacao",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Localizacao",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            // Inserção condicional dos dados iniciais (seeding)
            migrationBuilder.Sql(
                @"
                INSERT INTO ""TipoUtilizador"" (""Tipo"")
                SELECT 'ADMIN'
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""TipoUtilizador"" WHERE ""Tipo"" = 'ADMIN'
                );

                INSERT INTO ""TipoUtilizador"" (""Tipo"")
                SELECT 'USER_MANAGER'
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""TipoUtilizador"" WHERE ""Tipo"" = 'USER_MANAGER'
                );

                INSERT INTO ""TipoUtilizador"" (""Tipo"")
                SELECT 'USER'
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""TipoUtilizador"" WHERE ""Tipo"" = 'USER'
                );
                ");

            var adminPasswordHash = PasswordHelper.HashPassword("admin123");
            migrationBuilder.Sql(
                $@"INSERT INTO ""Utilizadores"" (""UtilizadorId"", ""Username"", ""Email"", ""Password"", ""TipoUtilizadorId"", ""DataCriacao"", ""Cargo"", ""GoogleToken"", ""Telefone"", ""GoogleId"", ""Pontos"")
                SELECT COALESCE((SELECT MAX(""UtilizadorId"") FROM ""Utilizadores""), 0) + 1, 'admin', 'admin@example.com', '{adminPasswordHash}', 
                       (SELECT ""TipoUtilizadorId"" FROM ""TipoUtilizador"" WHERE ""Tipo"" = 'ADMIN'), 
                       CURRENT_TIMESTAMP, NULL, NULL, NULL, NULL, 0
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Utilizadores"" u
                    JOIN ""TipoUtilizador"" t ON u.""TipoUtilizadorId"" = t.""TipoUtilizadorId""
                    WHERE t.""Tipo"" = 'ADMIN'
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GoogleId",
                table: "Utilizadores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Conteudo",
                table: "Mensagens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Localizacao",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,6)",
                oldPrecision: 10,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Localizacao",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,6)",
                oldPrecision: 10,
                oldScale: 6,
                oldNullable: true);
        }
    }
}
