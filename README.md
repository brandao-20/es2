# ES2_TP_ComparadorPrecos

Plataforma de Comparação de Preços desenvolvida em .NET 8 (Web API + Blazor).

## Como executar
1. Instalar .NET 8 e PostgreSQL
2. Criar a BD ou deixar o EF criar via migrations
3. `dotnet ef migrations add InitialCreate`
4. `dotnet ef database update`
5. `dotnet run`

## Estrutura
- `WebAPI`: Projeto Web API (backend)
- `WebApp`: Projeto Blazor (frontend)
