using DatabaseManagement;
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
        var selected = await context.Set<UserData>()
            .Where(i => i.DiscordId == discordId)
            .Select(i => new Home.HomePageData
            {
                FavoriteAvatarUrl = Player.GetImageUrl(i.Gender),
                Coins = i.Items.Where(j => j is Coin)
                    .Select(j => new int?(j.Stacks)).FirstOrDefault() ?? 0,
                DivineShards = i.Items.Where(j => j is DivineShard)
                    .Select(j => new int?(j.Stacks)).FirstOrDefault() ?? 0,
                Stamina = i.Items.Where(j => j is Stamina)
                    .Select(j => new int?(j.Stacks)).FirstOrDefault() ?? 0,
                AdventurerLevel = i.AdventurerLevel
            }).FirstOrDefaultAsync();
        if (selected is null)
        {
            return BadRequest("User data not found. You have not started battle");
        }
        return Ok(selected);
    }
}