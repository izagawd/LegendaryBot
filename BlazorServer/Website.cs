using System.Globalization;
using AspNet.Security.OAuth.Discord;
using BlazorServer.WebsiteStuff;
using DatabaseManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PublicInfo;
using _Imports = WebAssemblyApp._Imports;

namespace BlazorServer;

public static class Website
{
    public static async Task Main(string[] args)
    {
        await StartAsync(args);
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        services.AddScoped(i => new HttpClient { BaseAddress = new Uri(Information.DomainName) });

        services.AddDbContext<PostgreSqlContext>();
        services.AddSession(i => { i.IdleTimeout = TimeSpan.MaxValue; }
        );


        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
            })
            .AddDiscord(options =>
            {
                options.ClientId = "340054610989416460";
                options.ClientSecret = "n-Jy3ogvEmMnaFRIVmguqzpLgW8pYp2m";
                options.SaveTokens = true;
                options.CallbackPath = "/signin-discord";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                        user.GetString("id"),
                        user.GetString("avatar"),
                        user.GetString("avatar")!.StartsWith("a_") ? "gif" : "png"));
            })
            .AddCookie(options => { }).AddCertificate(options => { options.Validate(); });
    }


    public static async Task<bool> IsLoadedAsync()
    {
        try
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    if (cert is not null && !cert.Verify())
                        return httpRequestMessage.RequestUri is not null
                               && httpRequestMessage.RequestUri.ToString().Contains(Information.DomainName);
                    return cert is not null;
                };
            using var webClient = new HttpClient(handler);

            var checkingResponse = await webClient.GetAsync(Information.DomainName);
            return checkingResponse.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static async Task StartAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseStaticWebAssets();
        ConfigureServices(builder.Services);

        var app = builder.Build();


        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCertificateForwarding();
        app.UseRouting();
        app.UseAuthentication();
        app.UseRouting();
        app.UseAntiforgery();
        app.UseAuthorization();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(_Imports).Assembly);


        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        app.UseSession();

        await app.RunAsync(Information.DomainName);
    }
}