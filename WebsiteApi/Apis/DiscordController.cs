using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebFunctions;

namespace WebsiteApi.ClientSenders;

[Route("[controller]")]
public class DiscordController : Controller
{
    [HttpGet("login")]
    public IActionResult Login([FromQuery]string redirectUri)
    {
        return Challenge(new AuthenticationProperties()
        { 
RedirectUri = redirectUri
        }, "Discord");
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
        if (!User.Identity?.IsAuthenticated ?? false) return Unauthorized();
        return Ok(User.Claims.DistinctBy(i => i.Type).ToDictionary(i => i.Type, i => i.Value));
    }
}