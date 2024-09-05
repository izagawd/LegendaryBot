using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PublicInfo;
using Website.Services;

namespace Website;

public class Program
{
    public static async Task Main(string[] args)
    {
        
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddBlazoredSessionStorage();
        builder.Services.AddScoped<WebsiteThemeService>();
        builder.Services.AddScoped<DiscordTokenService>();
        builder.Services
            .AddScoped(sp =>
            {
                var dep = sp.GetRequiredService<DiscordTokenService>();
              
                var client = new HttpClient
                {
                    BaseAddress =
                        new Uri(Information.ApiDomainName)
                };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                     dep.DiscordToken);
                return client;

            });
        builder.Services.AddScoped<AuthenticationStateProvider, DiscordAuthenticationStateProvider>();
        builder.Services.AddAuthorizationCore();

        builder.Services.AddScoped<NavigationService>();
        await builder.Build().RunAsync();
    }
}