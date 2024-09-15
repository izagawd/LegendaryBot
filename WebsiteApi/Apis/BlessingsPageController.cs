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
            .ToArrayAsync();


        var gottenCollection = gottenCollectionTypeIds
            .GroupBy(i => i.TypeId)
            .Select(i => new Blessings.BlessingDto
            {
                Name = i.First().Name,
                RarityNum = (int)i.First().Rarity,
                Stacks = i.Count(),
                Description = i.First().Description,
                TypeId = i.Key
            }).ToArray();
        return Ok(gottenCollection);
    }
}