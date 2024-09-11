using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PublicInfo;

namespace Website;

public class DiscordAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;

    public DiscordAuthenticationStateProvider(HttpClient client)
    {
        _httpClient = client;
    }


    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get,
            Information.ApiDomainName + "/Discord/get-data");
        var sent = await _httpClient.SendAsync(req);
        if (sent.StatusCode == HttpStatusCode.Unauthorized || !sent.IsSuccessStatusCode)
            return new AuthenticationState(new ClaimsPrincipal());


        var content = await sent.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        if (content is null) return new AuthenticationState(new ClaimsPrincipal());
        return new AuthenticationState(new ClaimsPrincipal(
            new ClaimsIdentity(content.Select(i =>
                new Claim(i.Key, i.Value)), "Discord")));
    }
}