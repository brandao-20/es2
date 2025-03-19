using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebApp;
using WebApp.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient para a API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000/") });

// Registrar o AuthService para gerenciar usuário/role/token
builder.Services.AddScoped<AuthService>();

// Registrar Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
