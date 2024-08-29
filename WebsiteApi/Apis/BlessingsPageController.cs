using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAssemblyApp.Pages.Characters;
using WebFunctions;
using Website.Pages.Blessings;
using Website.Pages.Gears;

namespace WebsiteApi.ClientSenders;

[Route("[controller]")]
[ApiController] 
public class BlessingsPageController : ControllerBase
{
    
    [HttpGet("get-blessings")]
    [Authorize]
    public async Task<IActionResult> GetCharactersAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? false)
            return Unauthorized();
        await using var post = new PostgreSqlContext();
        var zaId = User.GetDiscordUserId();
        var gottenCollectionTypeIds = await post.Blessings
            .Where(i => i.UserData.DiscordId == zaId)
            .Select(i => i.TypeId)
            .ToArrayAsync();


        var gottenCollection = gottenCollectionTypeIds.GroupBy(i => i).Select(i =>
        {
            return new BlessingPageDto()
            {
                ImageUrl = Blessing.GetDefaultFromTypeId(i.Key).ImageUrl,
                Name = Blessing.GetDefaultFromTypeId(i.Key).Name,
                RarityNum = (int)Blessing.GetDefaultFromTypeId(i.Key).Rarity,
                Stacks = i.Count(),
                TypeId = i.Key
            };
        }).ToArray();
        return Ok(gottenCollection);
    }
}