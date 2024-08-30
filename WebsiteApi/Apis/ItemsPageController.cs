using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Blessings;
using Website.Pages.Items;

namespace WebsiteApi.Apis;

[Route("[controller]")]
[ApiController] 
public class ItemsPageController : ControllerBase
{
    
    [HttpGet("get-items")]
    [Authorize]
    public async Task<IActionResult> GetCharactersAsync()
    {
        await using var post = new PostgreSqlContext();
        var zaId = User.GetDiscordUserId();
        var gottenCollection = await post.Items
            .Where(i => i.UserData.DiscordId == zaId)
            .Select(i => new ItemPageDto()
            {
                ImageUrl = Item.GetDefaultFromTypeId(i.TypeId).ImageUrl,
                Name = Item.GetDefaultFromTypeId(i.TypeId).Name,
                Stacks = i.Stacks,
                RarityNum = (int) i.Rarity,
                TypeId = i.TypeId
            })
            .ToArrayAsync();

        return Ok(gottenCollection);
    }
}