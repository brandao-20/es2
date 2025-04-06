using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Context;
using WebAPI.Factories;
using WebAPI.Hubs;
using WebAPI.Repositories;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registo dos serviços
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddSignalR();

// Registo dos repositórios
builder.Services.AddScoped<IUtilizadorRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateUtilizadorRepository());
builder.Services.AddScoped<IProdutoRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateProdutoRepository());
builder.Services.AddScoped<ICategoriaRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateCategoriaRepository());
builder.Services.AddScoped<ILojaRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateLojaRepository());
builder.Services.AddScoped<ILocalizacaoRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateLocalizacaoRepository());
builder.Services.AddScoped<IRegistosPrecoRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateRegistosPrecoRepository());
builder.Services.AddScoped<ITipoAcaoRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateTipoAcaoRepository());
builder.Services.AddScoped<ITipoUtilizadorRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateTipoUtilizadorRepository());
builder.Services.AddScoped<IMensagemRepository>(sp =>
    sp.GetRequiredService<IRepositoryFactory>().CreateMensagemRepository());

// Configuração dos controllers, Swagger e CORS
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5116")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Configuração de autenticação
var jwtSettings = builder.Configuration.GetSection("Jwt");
string jwtKey = jwtSettings["Key"] ??
    "cf7fe7d90327ce76c4f697bfb31f1e1fe11cd98c484af55e9fe5b9e9fe10d35d09ff4ea95c763a91d4fbae68b348c8b2f32b29dd57f349d42b23aa3749cbac8adf59b35f9093a54b28c92d1b17f8a06fd65a7aa6a6331507d4366656823c40d50d43c597bdfd659098e3ddddfe75bcd923f4a47399001d1c5ab17bda70c69defc8e0a463030bb75f7d0610cff50aea4ffbbf64b101a6481cac42b8dca368ee368dadbe7f9ac88db6dc5476aefa8c0d5c67f0a18d0483eb3b056e93eb4dc51384f2d64abbe5fa74432545d0bd31cdc173c2f85fc2019bb154418c5cd59bb1400419d57557ac14a3284a9e40977975545efc2338eb4ba810ac4e0b7b32c7c49688";
string jwtIssuer = jwtSettings["Issuer"] ?? "http://localhost:5000";
string jwtAudience = jwtSettings["Audience"] ?? "http://localhost:5000";

Console.WriteLine($"[DEBUG] jwtKey='{jwtKey}' (length={jwtKey.Length})");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = "ExternalCookies";
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
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[ERROR] Autenticação falhou: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("[DEBUG] Token validado com sucesso.");
            return Task.CompletedTask;
        }
    };
})
.AddCookie("ExternalCookies", o =>
{
    o.ExpireTimeSpan = TimeSpan.FromMinutes(60);
})
.AddGoogle(options =>
{
    options.SignInScheme = "ExternalCookies";
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.Scope.Add("profile");
    options.Scope.Add("email");
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware de tratamento de exceções
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            await context.Response.WriteAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Ocorreu um erro interno. Tente novamente mais tarde."
            }.ToString());
        }
    });
});

// Configuração do pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("DevCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chathub").RequireAuthorization();
});

app.Run();
