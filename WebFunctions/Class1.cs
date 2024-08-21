using System.Security.Claims;

namespace WebFunctions;

public static class Functions
{
    public static string GetDiscordUserName(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
        return claim?.Value!;
    }

    public static ulong GetDiscordUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(i =>
            i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return ulong.Parse(claim!.Value);
    }

    public static string GetDiscordUserAvatarUrl(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(i =>
            i.Type == "urn:discord:avatar:url");
        return claim?.Value!;
    }

}