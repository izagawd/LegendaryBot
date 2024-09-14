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
public class GearsPageController(PostgreSqlContext post) : ControllerBase
{
    [HttpGet("get-gears")]
    [Authorize]
    public async Task<IActionResult> GetGearsAsync()
    {
        var zaId = User.GetDiscordUserId();
        var gottenCollection = await post.Set<Gear>()
            .Where(i => i.UserData!.DiscordId == zaId)
         
            .ToArrayAsync();
        var dto = gottenCollection.Select(i => new Gears.GearDto
        {
            Name = i.Name,
            Number = i.Number,
            RarityNum = (int)i.Rarity,
            TypeId = i.TypeId
        }).ToArray();
        return Ok(dto);
    }
}