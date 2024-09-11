using System.Security.Claims;

namespace WebFunctions;

public static class Functions
{
    public static ulong GetDiscordUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("id");
        return ulong.Parse(claim!.Value);
    }
}