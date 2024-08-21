using BattleManagemen.LegendaryBot;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAssemblyApp.Pages.Characters;

namespace BlazorServer.ClientSenders;

[ApiController]
public class CharactersPageSender : ControllerBase
{
    private static readonly int PlayerTypeId = TypesFunction.GetDefaultObject<Player>().TypeId;

    [Route("api/characters-collection")]
    [HttpGet]
    public async Task<IActionResult> GetCharactersAsync([FromQuery] ulong discordId)
    {
        await using var post = new PostgreSqlContext();
        var gottenCollection = await post.Characters.Where(i => i.UserData.DiscordId == discordId)
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