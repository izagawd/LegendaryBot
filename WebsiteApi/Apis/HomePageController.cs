using DatabaseManagement;
using Entities.LegendaryBot.Entities.Items;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;

namespace WebsiteApi.Apis;
[ApiController]
[Route("[controller]")]
public class HomePageController : ControllerBase
{
    [Authorize]
    public async Task<IActionResult> GetSomeUserData()
    {

        await using var context = new PostgreSqlContext();
        var discordId = User.GetDiscordUserId();
        var selected = await context.Set<UserData>()
            .Where(i => i.DiscordId == discordId)
            .Select(i => new
            {
                coinsCount = i.Items.Where(j => j is Coin)
                    .Select(j => j.Stacks).FirstOrDefault(0),
                divineShardsCount = i.Items.Where(j => j is DivineShard)
                    .Select(j => j.Stacks).FirstOrDefault(0),
                staminaValue = i.Items.Where(j => j is Stamina)
                    .Select(j => j.Stacks).FirstOrDefault(0)
            }).FirstOrDefaultAsync();
        if (selected is null)
        {
            selected = new { coinsCount = 0, divineShardsCount = 0, staminaValue =0 };
        }

        return Ok(selected);
    }
}