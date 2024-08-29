using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PublicInfo;
using WebsiteShared;

namespace Website;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddBlazoredLocalStorage();

        builder.Services.AddScoped<WebsiteThemeService>();

        builder.Services
            .AddScoped(sp =>
            {
                var locStor = sp.GetRequiredService<ISyncLocalStorageService>();
                var client = new HttpClient
                {
                    BaseAddress =
                        new Uri(Information.ApiDomainName)
                };
                client.DefaultRequestHeaders.Add("Access-Control-Allow-Flight","*");
                client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin","*");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                     locStor.GetItem<string>(WebsiteConstants.DiscordTokenKey));
                return client;

            });
        builder.Services.AddScoped<AuthenticationStateProvider, DiscordAuthenticationStateProvider>();
        builder.Services.AddAuthorizationCore();


        await builder.Build().RunAsync();
    }
}