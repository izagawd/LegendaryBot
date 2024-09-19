using CommandLine;
using DatabaseManagement;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.Items;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages;

namespace WebsiteApi.Apis;

[ApiController]
[Route("[controller]")]
public class HomePageController(PostgreSqlContext context) : ControllerBase
{
    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult> GetSomeUserData()
    {
        var discordId = User.GetDiscordUserId();
        var userData = await context.Set<UserData>()
            .Include(i => i.Characters.Where(j => j is Player))
            .Include(i => i.Items.Where(j => j is Coin || j is DivineShard || j is Stamina))
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);

        if (userData is null) return BadRequest("Userdata not found");
        if (userData.Tier == Tier.Unranked) return BadRequest("You have not yet started your journey with /begin");
        var stamina = userData.Items.FirstOrDefault(i => i is Stamina)
            ?.Cast<Stamina>().Stacks ?? 0;
        var selected = new Home.HomePageData
        {
            AdventurerLevel = userData.AdventurerLevel,
            Stamina = stamina,
            Coins = userData.Items.OfType<Coin>().Select(i => i.Stacks)
                .FirstOrDefault(0),
            DivineShards = userData.Items.OfType<DivineShard>().Select(i => i.Stacks)
                .FirstOrDefault(0),
            FavoriteAvatarUrl = userData.Characters.OfType<Player>().Select(i => i.ImageUrl)
                .FirstOrDefault()!
        };

        return Ok(selected);
    }
}