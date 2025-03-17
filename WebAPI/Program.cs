using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ES2_TP_ComparadorPrecos_WebAPI.Context;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adicionar serviços de Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Lê as configurações do JWT do appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt");
string jwtKey = jwtSettings["Key"] ?? "chave_default_muito_segura_aqui";
string jwtIssuer = jwtSettings["Issuer"] ?? "https://meusite.com";
string jwtAudience = jwtSettings["Audience"] ?? "https://meusite.com";

// Configurar autenticação usando JWT e Google OAuth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

// Adicionar autorização
builder.Services.AddAuthorization();

// Construir o aplicativo
var app = builder.Build();

// Middleware global para tratamento de exceções
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            // Aqui você pode formatar a resposta conforme necessário.
            await context.Response.WriteAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Ocorreu um erro interno. Tente novamente mais tarde."
            }.ToString());
        }
    });
});

// Configurar o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ativar autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Mapear os controllers
app.MapControllers();

// Executar a aplicação
app.Run();
