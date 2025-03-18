# ES2_TP_ComparadorPrecos

Plataforma de Comparação de Preços desenvolvida em .NET 8 (Web API + Blazor).

## Como executar
1. Instalar .NET 8 e PostgreSQL.
2. Criar a base de dados ou permitir que o EF Core crie via migrations:
   - `dotnet ef migrations add InitialCreate`
   - `dotnet ef database update`
3. Executar o backend:
   - Navegue até o diretório `WebAPI` e execute `dotnet run`.
4. Executar o frontend:
   - Navegue até o diretório `WebApp` e execute `dotnet run`.
5. Acesse a aplicação através do navegador no endereço configurado (por exemplo, http://localhost:5116).

## Estrutura
- **WebAPI**: Projeto Web API (backend) com implementação de autenticação, CRUD e relatórios.
- **WebApp**: Projeto Blazor (frontend) com interface aprimorada para interação com o backend.
