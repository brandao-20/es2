using ES2_TP_ComparadorPrecos_WebApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Define a base address para a tua API 
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5029/") });

await builder.Build().RunAsync();