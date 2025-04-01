# ES2_TP_ComparadorPrecos

Plataforma de Comparação de Preços desenvolvida em .NET 8. Este projeto permite comparar preços de produtos em diferentes lojas, com autenticação, CRUD e relatórios.

## Pré-requisitos

Antes de começar, certifique-se de que você tem os seguintes itens instalados:

- **.NET 8 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verifique a versão instalada com:
    ```bash
    dotnet --version
    ```

- **PostgreSQL**: [Download](https://www.postgresql.org/download/)
   - Durante a instalação, configure um utilizador e password (por exemplo, utilizador postgres e password sua_password).

- **Git**: [Download](https://git-scm.com/downloads)
   - Para clonar o repositório.

## Configuração do Ambiente

Siga os passos abaixo para configurar e executar o projeto.

- **1. Clonar o Repositório**:

Clone o repositório para o seu computador:

   ```bash
   git clone <URL_DO_REPOSITORIO>
   cd ES2_TP_ComparadorPrecos
   ```

- **2. Configurar a Base de Dados**:

Conecte-se ao PostgreSQL (usando psql ou pgAdmin) e crie a base de dados ES2:

   ```bash
      CREATE DATABASE ES2;
   ```

- **3. Aplicar as Migrações**:

O projeto já inclui migrações para criar as tabelas necessárias. Siga os passos abaixo:

   1. Navegue até o diretório WebAPI:
      ```bash
         cd WebAPI
      ```

   2. Restaure as dependências do projeto:
      ```bash
         dotnet restore
      ```

   3. Aplique as migrações para criar as tabelas na base de dados:
      ```bash
         dotnet ef database update --project WebAPI.csproj
      ```

- **4. Configurar o appsettings.json**:

Copie o arquivo de exemplo para criar o appsettings.json:

   1. Navegue até o diretório WebAPI:
      ```bash
         cp WebAPI/appsettings.example.json WebAPI/appsettings.json
      ```

   2. Abra o arquivo WebAPI/appsettings.json e substitua os valores pelas suas credenciais:
      ```bash
         "ConnectionStrings": {
            "DefaultConnection": "Host=localhost;Database=ES2;Username=postgres;Password=sua_password"
         }
      ```

- **5. Executar o Backend**:

   1. No diretório WebAPI, execute o backend:
      ```bash
         dotnet run --launch-profile WebAPI
      ```

- **6. Executar o Frontend**:

   1. Abra um novo terminal e navegue até o diretório WebApp:
      ```bash
         cd WebApp
      ```

   2. Restaure as dependências do frontend:
      ```bash
         dotnet restore
      ```

   3. Execute o frontend:
      ```bash
         dotnet run
      ```

## Aceder à Aplicação

Abra o navegador e aceda ao frontend (http://localhost:5116)