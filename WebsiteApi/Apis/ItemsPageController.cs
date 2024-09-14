using DatabaseManagement;
using Entities.LegendaryBot.Entities.Items;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Items;

namespace WebsiteApi.Apis;

[Route("[controller]")]
[ApiController]
public class ItemsPageController(PostgreSqlContext post) : ControllerBase
{
    [HttpGet("get-items")]
    [Authorize]
    public async Task<IActionResult> GetCharactersAsync()
    {
        var zaId = User.GetDiscordUserId();
        var gottenCollection = await post.Set<Item>()
            .Where(i => i.UserData!.DiscordId == zaId)
            
            .ToArrayAsync();

        foreach (var i in gottenCollection.OfType<Energy>())
        {
            i.RefreshEnergyValue();
        }
        var dto = gottenCollection.Select(i => new Items.ItemDto
        {
   
            Name = Item.GetDefaultFromTypeId(i.TypeId).Name,
            Stacks = i.Stacks,
            RarityNum = (int)i.Rarity,
            TypeId = i.TypeId,
            Description = i.Description
        }).ToArray();
        return Ok(dto);
    }
}