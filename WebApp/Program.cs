using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using WebApp;
using WebApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();
builder.Services.AddTransient<AuthMessageHandler>();

// Registar HttpClient com o AuthMessageHandler
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthMessageHandler>();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5000/")
    };
});

// Registar o HubConnection como singleton, mas criando um escopo no AccessTokenProvider
builder.Services.AddSingleton(sp =>
{
    var hubConnection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5000/chathub", options =>
        {
            options.AccessTokenProvider = async () =>
            {
                using (var scope = sp.CreateScope())
                {
                    var localStorage = scope.ServiceProvider.GetRequiredService<ILocalStorageService>();
                    var token = await localStorage.GetItemAsync<string>("authToken");
                    Console.WriteLine($"[DEBUG] Token enviado para SignalR: {token}");
                    return token;
                }
            };
        })
        .WithAutomaticReconnect()
        .Build();
    return hubConnection;
});

await builder.Build().RunAsync();
