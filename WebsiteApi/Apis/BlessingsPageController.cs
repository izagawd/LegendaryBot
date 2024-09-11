using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Blessings;

namespace WebsiteApi.Apis;

[Route("[controller]")]
[ApiController]
public class BlessingsPageController(PostgreSqlContext post) : ControllerBase
{
    [HttpGet("get-blessings")]
    [Authorize]
    public async Task<IActionResult> GetCharactersAsync()
    {
        var zaId = User.GetDiscordUserId();
        var gottenCollectionTypeIds = await post.Set<Blessing>()
            .Where(i => i.UserData!.DiscordId == zaId)
            .Select(i => i.TypeId)
            .ToArrayAsync();


        var gottenCollection = gottenCollectionTypeIds.GroupBy(i => i).Select(i => new Blessings.BlessingDto
        {
            ImageUrl = Blessing.GetDefaultFromTypeId(i.Key).ImageUrl,
            Name = Blessing.GetDefaultFromTypeId(i.Key).Name,
            RarityNum = (int)Blessing.GetDefaultFromTypeId(i.Key).Rarity,
            Stacks = i.Count(),
            TypeId = i.Key
        }).ToArray();
        return Ok(gottenCollection);
    }
}