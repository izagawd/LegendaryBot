using Blazored.LocalStorage;
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
            .AddScoped(sp => new HttpClient { BaseAddress =
                new Uri(Information.ApiDomainName) });
        await builder.Build().RunAsync();
    }
}