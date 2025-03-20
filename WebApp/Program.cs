using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebApp;
using WebApp.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Registrar Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Registrar AuthService
builder.Services.AddScoped<AuthService>();

// Registrar o AuthMessageHandler (que já define seu InnerHandler internamente)
builder.Services.AddTransient<AuthMessageHandler>();

// Registrar HttpClient usando o AuthMessageHandler
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthMessageHandler>();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5000/")
    };
});

await builder.Build().RunAsync();
