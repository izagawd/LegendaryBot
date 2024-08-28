using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PublicInfo;

namespace Website;

public class ClientAuthStateProvider : AuthenticationStateProvider
{
    public const string DiscordAccessTokenKey = "discord_access_token";
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;
    private readonly NavigationManager _navigationManager;
    
    public ClientAuthStateProvider(ILocalStorageService localStorage, HttpClient http, NavigationManager navMan)
    {
        _localStorage = localStorage;
        _http = http;
        _navigationManager = navMan;
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {

        var token = await _localStorage.GetItemAsStringAsync(DiscordAccessTokenKey);
        if (token is null)
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }


  

        var user = new ClaimsPrincipal();
        var state = new AuthenticationState(user);
        Dictionary<string,string>? content;
        try
        {

            content = await _http.GetFromJsonAsync<Dictionary<string,string>>("Discord/get-user-data");
            user.AddIdentity(new ClaimsIdentity(content.Select(i => new Claim(i.Key,i.Value))));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return state;
        }
        NotifyAuthenticationStateChanged(Task.FromResult(state));
        return state;
    }
}