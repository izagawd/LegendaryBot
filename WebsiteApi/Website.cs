using DatabaseManagement;
using Microsoft.AspNetCore.Authentication;
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
            .AddScheme<AuthenticationSchemeOptions, DiscordAuthenticationHandler>(DiscordAuthScheme,
                i => { })
            .AddCertificate(options => { options.Validate(); });
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

      

        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();


        var stringToUse = Information.IsTesting ? Information.ApiDomainName : "http://localhost:5000";
        
        await app.RunAsync(stringToUse);
    }
}