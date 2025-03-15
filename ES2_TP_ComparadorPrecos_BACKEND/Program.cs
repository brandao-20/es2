using ES2_TP_ComparadorPrecos_BACKEND.models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Ler a connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Adicionar o DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. Adicionar serviços de Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Mapear Controllers
app.MapControllers();

app.Run();