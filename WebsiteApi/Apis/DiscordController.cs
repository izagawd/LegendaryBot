using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Nodes;
using AspNet.Security.OAuth.Discord;
using BasicFunctionality;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicInfo;
using WebFunctions;

namespace WebsiteApi.ClientSenders;

[Route("[controller]")]
[ApiController]
public class DiscordController : ControllerBase
{

    private HttpClient _httpClient;

    public DiscordController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet("get-data")]
    [Authorize]
    public async Task<IActionResult> TryGetDataAsync()
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? false)
        {
            return Unauthorized();
        }
        return Ok(HttpContext.User.Claims.DistinctBy(i => i.Type).ToDictionary(i => i.Type, i => i.Value));
    }
    [HttpGet("get-token")]
    public async Task<IActionResult> GetTokenAsync([FromQuery] string code, [FromQuery] string redirectUri)
    {
        var clientSecret = PrivateInfo.Information.ClientSecret;
        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>
            ("redirect_uri",redirectUri)
                ,
            new KeyValuePair<string, string>("client_id",Information.DiscordClientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),

        });
        
        var request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/v10/oauth2/token")
        {
            Content = requestContent
        };


        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {

            var tokenResponse = await response.Content.ReadFromJsonAsync<JsonObject>();
    
            return Ok(tokenResponse);
        }
        return BadRequest();
        
    }

}