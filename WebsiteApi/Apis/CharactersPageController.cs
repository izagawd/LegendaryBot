using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Characters;

namespace WebsiteApi.Apis;

[Route("[controller]")]
[ApiController]
public class CharactersPageController(PostgreSqlContext post) : ControllerBase
{
    [HttpGet("get-characters")]
    [Authorize]
    public async Task<IActionResult> GetCharactersAsync()
    {
        var zaId = User.GetDiscordUserId();
        var userData = await post.Set<UserData>()
            .AsNoTrackingWithIdentityResolution()
            .Include(i => i.Characters)
            .FirstOrDefaultAsync(i => i.DiscordId == zaId);

        if (userData is null) return Ok(new Characters.CharacterDto[] { });
        var toDto = userData.Characters
            .Select(i => new Characters.CharacterDto
            {
                Name = i.Name,
                Level = i.Level,
                TypeId = i.TypeId,
         
                RarityNum = (int)Character.GetDefaultFromTypeId(i.TypeId).Rarity
            })
            .ToArray();

        return Ok(toDto);
    }
}