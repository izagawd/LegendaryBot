using BasicFunctionality;
using BattleManagemen.LegendaryBot;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAssemblyApp.Pages.Characters;
using WebFunctions;

namespace BlazorServer.ClientSenders;

[Microsoft.AspNetCore.Mvc.Route("[controller]")]
[ApiController]
public class CharactersPageController : ControllerBase
{

    [HttpGet("anime")]
    public async Task<IActionResult> GetJson()
    {
        return Ok(new { Name = "emmanuel", Age = 3 });
    }
    private static readonly int PlayerTypeId = TypesFunction.GetDefaultObject<Player>().TypeId;

    [HttpGet("get-characters")]
    [Authorize]
    public async Task<ActionResult<CharacterPageDto[]>> GetCharactersAsync()
    {
        Response.Headers.Authorization.ForEach(i => i.Print());
        await using var post = new PostgreSqlContext();

        var discordId = User.GetDiscordUserId();
        var gottenCollection = await post.Characters
            .Where(i => i.UserData.DiscordId == discordId)
            .Select(i => new CharacterPageDto
            {
                ImageUrl = i.TypeId == PlayerTypeId
                    ? Player.GetImageUrl(i.UserData.Gender)
                    : Character.GetDefaultCharacterFromTypeId(i.TypeId).ImageUrl,
                Level = i.Level,
                Name = i.TypeId == PlayerTypeId
                    ? i.UserData.Name
                    : Character.GetDefaultCharacterFromTypeId(i.TypeId).Name,
                Number = i.Number,

                RarityNum = (int)Character.GetDefaultCharacterFromTypeId(i.TypeId).Rarity
            })
            .ToArrayAsync();

        return Ok(gottenCollection);
    }
}