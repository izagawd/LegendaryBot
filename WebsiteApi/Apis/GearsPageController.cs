using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Gears;

namespace WebsiteApi.Apis;

[Route("[controller]")]
[ApiController] 
public class GearsPageController : ControllerBase
{
    
    [HttpGet("get-gears")]
    [Authorize]
    public async Task<IActionResult> GetGearsAsync()
    {

        await using var post = new PostgreSqlContext();
        var zaId = User.GetDiscordUserId();
        var gottenCollection = await post.Gears
            .Where(i => i.UserData!.DiscordId == zaId)
            .Select(i => new Gears.GearDto()
            {
                ImageUrl = Gear.GetDefaultFromTypeId(i.TypeId).ImageUrl,
                Name = Gear.GetDefaultFromTypeId(i.TypeId).Name,
                Number = i.Number,
                RarityNum = (int) i.Rarity,
                TypeId = i.TypeId
            })
            .ToArrayAsync();

        return Ok(gottenCollection);
    }
}