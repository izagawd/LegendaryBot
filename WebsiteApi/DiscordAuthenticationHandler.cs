using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using BasicFunctionality;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace WebsiteApi;


public class DiscordAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected HttpClient _httpClient;

    public DiscordAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,HttpClient httpClient)
        : base(options,logger,encoder)
    {
        _httpClient = httpClient;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
  
        // Implement your custom authentication logic here
        // For example, check for a specific header or token

        var token = Request.Headers.Authorization.FirstOrDefault()?.Split(" ").LastOrDefault();

        if (token is null)
        {
            return AuthenticateResult.Fail("No token found");
        }
        var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/users/@me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var sent = await _httpClient.SendAsync(request);
        if (!sent.IsSuccessStatusCode)
        {
            return AuthenticateResult.Fail("Invalid token");
        }

        var discordData = await sent.Content.ReadFromJsonAsync<JsonObject>();
        if (discordData is null)
        {
            return AuthenticateResult.Fail("");
        }
        Console.WriteLine(token);
        return AuthenticateResult.Success(
            new AuthenticationTicket(
                new ClaimsPrincipal(
                    new ClaimsIdentity([
                        new Claim("id", discordData["id"]!.GetValue<string>()),
                        new Claim("username", discordData["username"]!.GetValue<string>())
                    ],Website.DiscordAuthScheme)
                )
                , Website.DiscordAuthScheme)
        );

    }
}