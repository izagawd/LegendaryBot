using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PublicInfo;

namespace WebAssemblyApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddAuthorizationCore();



        builder.Services.AddScoped(sp => new HttpClient(){BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

        builder.Services.AddScoped<AuthenticationStateProvider, ClientAuthenticationStateProvider>();
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("Discord", options.ProviderOptions);
            // Or configure OAuth options for Discord here
        });
        builder.Services.AddApiAuthorization();
        await builder.Build().RunAsync();
    }
}