using System.Collections.Immutable;
using System.Diagnostics;
using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;
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
            
            .AsNoTrackingWithIdentityResolution()
            .Include(i => i.Stats)
            .Include(i => i.Character)
            .Where(i => i.UserData!.DiscordId == zaId)
            .ToArrayAsync();

        var dto = gottenCollection.Select(i => new Gears.GearDto
        {
            Name = i.Name,
            Number = i.Number,
            RarityNum = (int)i.Rarity,
            TypeId = i.TypeId,
            OwnerTypeId = i.Character?.TypeId,
            GearStatDtos = i.Stats.Select(j => new Gears.GearStatDto
            {
                IsMainStat = j.IsMainStat is not null,
                Value = j.Value,
                TypeId = j.TypeId,
                IsPercentage = j.IsPercentage
            }).ToArray()
        }).ToArray();

        return Ok(new Gears.GearPageDto
        {
            Gears = dto,
            GearStatNameMapper = TypesFunction.GetDefaultObjectsAndSubclasses<GearStat>()
                .ToImmutableDictionary(i => i.TypeId, i => i.StatType.GetShortName())
        });
    }
}