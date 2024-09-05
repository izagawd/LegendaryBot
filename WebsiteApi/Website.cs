using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;


using DatabaseManagement;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Components.Authorization;
using PublicInfo;


namespace WebsiteApi;

public static class Website
{
    public const string DiscordAuthScheme = "Discord";
    public static void ConfigureServices(IServiceCollection services)
    {


        services.AddHttpClient("WebApi", i => i.BaseAddress =
            new Uri(Information.ApiDomainName));
        services.AddScoped(i => i.GetRequiredService<IHttpClientFactory>()
            .CreateClient());

        services.AddDbContext<PostgreSqlContext>();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins(Information.WebsiteDomainName)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
        });
     
        services.AddControllers();
        services.AddAuthentication(options =>
            {

                options.DefaultSignInScheme = null;
                options.DefaultScheme = DiscordAuthScheme;
                options.DefaultChallengeScheme = null;
                options.DefaultAuthenticateScheme = DiscordAuthScheme;
            })
            .AddScheme<AuthenticationSchemeOptions,DiscordAuthenticationHandler>(DiscordAuthScheme,
                i =>
                {
                    
                })
            .AddCertificate(options => { options.Validate(); });
        DiscordAuthenticationOptions options;
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
                               && httpRequestMessage.RequestUri.ToString().Contains(Information.ApiDomainName);
                    return cert is not null;
                };
            using var webClient = new HttpClient(handler);

            var checkingResponse = await webClient.GetAsync(Information.ApiDomainName);
            return checkingResponse.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static async Task StartAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args
        });
        builder.WebHost.UseStaticWebAssets();
        ConfigureServices(builder.Services);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
    
        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
       
        app.MapControllers();
        

        await app.RunAsync(Information.ApiDomainName);
    }
}