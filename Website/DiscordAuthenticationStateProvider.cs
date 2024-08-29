using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Website;

public class DiscordAuthenticationStateProvider : AuthenticationStateProvider
{


    private HttpClient _httpClient;
    public DiscordAuthenticationStateProvider(HttpClient client)
    {
        _httpClient = client;
        _authState = ProcessAuthStateAsync();
    }




    private Task<AuthenticationState> _authState;
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return _authState;
    }


    public async  Task<AuthenticationState> ProcessAuthStateAsync()
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
}