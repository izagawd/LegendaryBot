using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace WebAssemblyApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddOidcAuthentication(options =>
        {
            options.ProviderOptions.ClientId = "340054610989416460";
            options.ProviderOptions.Authority = "https://discord.com/api/oauth2/authorize";
            options.ProviderOptions.RedirectUri = "https://yourdomain.com/authentication/callback";
            options.ProviderOptions.ResponseType = "code";
            options.ProviderOptions.DefaultScopes.Add("identify");
            options.ProviderOptions.PostLogoutRedirectUri = "/";
        });
        await builder.Build().RunAsync();
    }


}