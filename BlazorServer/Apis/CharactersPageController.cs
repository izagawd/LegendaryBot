using System.Net.Http.Headers;
using AspNet.Security.OAuth.Discord;
using BasicFunctionality;
using BattleManagemen.LegendaryBot;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebAssemblyApp.Pages.Characters;
using WebFunctions;

namespace BlazorServer.ClientSenders;

[Route("[controller]")]
[ApiController]
public class CharactersPageController : Controller
{
    private static readonly int PlayerTypeId = TypesFunction.GetDefaultObject<Player>().TypeId;


    [HttpGet("get-characters")]

    public async Task<ActionResult<CharacterPageDto[]>> GetCharactersAsync([FromQuery] ulong discordId)
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