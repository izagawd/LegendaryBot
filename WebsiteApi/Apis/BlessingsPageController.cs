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
public class BlessingsPageController : ControllerBase
{
    
    [HttpGet("get-blessings")]
    [Authorize]
    public async Task<IActionResult> GetCharactersAsync()
    {
 
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