using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebFunctions;

namespace BlazorServer.ClientSenders;

[Route("[controller]")]
public class DiscordController : Controller
{
    [HttpGet("login")]
    public IActionResult Login()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("Callback")
        };
        return Challenge(properties, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
            return BadRequest(); // Handle failure

        // Get user info from the claims
        var userId = authenticateResult.Principal.GetDiscordUserId();
        var userName = authenticateResult.Principal.FindFirstValue("name");

        // Do something with the user information
        return Ok(new { userId, userName });
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet("get-user-data")]
    public async Task<IActionResult> GetUserData()
    {
        
        if (!User.Identity.IsAuthenticated)
        {
            return Login(); // User is not authenticated
        }
        // Get user info from the claims
        var userId = User.GetDiscordUserId(); // Assuming you have an extension method
        var userName = User.FindFirstValue("name");
        var dic = User.Claims.ToDictionary(i => i.Type, i => i.Value);
        dic["token"] = await HttpContext.GetTokenAsync("access_token");
        return Ok(dic);
    }
}