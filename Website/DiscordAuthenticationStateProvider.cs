using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Website;

public class DiscordAuthenticationStateProvider : AuthenticationStateProvider
{

    private Task<AuthenticationState> authState;
    private HttpClient _httpClient;
    public DiscordAuthenticationStateProvider(HttpClient client)
    {
        _httpClient = client;
        authState = TheProcess();
    }

    
    private async Task<AuthenticationState> TheProcess()
    {
        var req = new HttpRequestMessage(HttpMethod.Get,
            PublicInfo.Information.ApiDomainName + "/Discord/get-data");
        var sent = await _httpClient.SendAsync(req);
        if (sent.StatusCode == HttpStatusCode.Unauthorized || !sent.IsSuccessStatusCode)
        {
        
            return new AuthenticationState(new ClaimsPrincipal());
        } 
        
        
        var content =await sent.Content.ReadFromJsonAsync<Dictionary<string, string>>();
 
        return new AuthenticationState(new ClaimsPrincipal(
            new ClaimsIdentity(content.Select(i =>
                new Claim(i.Key,i.Value)),"Discord")));
    }
    public override  Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return authState;
    }
}