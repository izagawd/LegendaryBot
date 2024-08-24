using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace WebAssemblyApp;

// This is a client-side AuthenticationStateProvider that determines the user's authentication state by
// looking for data persisted in the page when it was rendered on the server. This authentication state will
// be fixed for the lifetime of the WebAssembly application. So, if the user needs to log in or out, a full
// page reload is required.
//
// This only provides a user name and email for display purposes. It does not actually include any tokens
// that authenticate to the server when making subsequent requests. That works separately using a
// cookie that will be included on HttpClient requests to the server.
internal class ClientAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string ClaimsKey = "claims";
    private readonly HttpClient _httpClient;
    
    private AuthenticationState _state;
    public ClientAuthenticationStateProvider(PersistentComponentState state)
    {
        if (state.TryTakeFromJson(ClaimsKey, out Dictionary<string, string> claims))
        {
            _state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
                claims.Select(i => new Claim(i.Key,i.Value)))));
        }
        else
        {
            throw new Exception();
        }
     
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return _state;
    }
}