using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;
using BlazorApp4.Components.Account;

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
    public static void ConfigureServices(IServiceCollection services)
    {


        services.AddHttpClient("WebApi", i => i.BaseAddress =
            new Uri(Information.ApiDomainName));
        services.AddScoped(i => i.GetRequiredService<IHttpClientFactory>()
            .CreateClient());
        services.AddScoped<AuthenticationStateProvider, LegendaryAuthenticationStateProvider>();
        services.AddDbContext<PostgreSqlContext>();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        
        services.AddControllers();
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddDiscord(options =>
            {
                options.ClientId = "340054610989416460";
                options.ClientSecret = "n-Jy3ogvEmMnaFRIVmguqzpLgW8pYp2m";
                options.SaveTokens = true;
                options.CallbackPath = "/signin-discord";
   
                options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                        user.GetString("id"),
                        user.GetString("avatar"),
                        user.GetString("avatar")!.StartsWith("a_") ? "gif" : "png"));
                options.Events.OnCreatingTicket = context =>
                {
                    context.Principal.AddIdentity(new ClaimsIdentity([
                        new Claim("discord_access_token",
                            context.AccessToken)
                    ]));
                    return Task.CompletedTask;
                };


            })
            .AddCookie(options => { })
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

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("AllowAllOrigins");
        app.MapControllers();
        

        await app.RunAsync(Information.ApiDomainName);
    }
}