using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Blessings;
using Website.Pages.Characters;

namespace WebsiteApi.Apis;

[Microsoft.AspNetCore.Mvc.Route("[controller]")]
[ApiController]
public class CharactersPageController(PostgreSqlContext post) : ControllerBase
{

    private static readonly int PlayerTypeId = TypesFunction.GetDefaultObject<Player>().TypeId;

    [HttpGet("get-characters")]
    [Authorize]
    public async Task<ActionResult<Characters.CharacterDto[]>> GetCharactersAsync()
    {

     
        var zaId = User.GetDiscordUserId();
        var gottenCollection = await post.Set<Character>()
            .Where(i => i.UserData!.DiscordId == zaId)
            .Select(i => new Characters.CharacterDto
            {
                ImageUrl = i.TypeId == PlayerTypeId
                    ? Player.GetImageUrl(i.UserData!.Gender)
                    : Character.GetDefaultFromTypeId(i.TypeId).ImageUrl,
                Level = i.Level,
                Name = i.TypeId == PlayerTypeId
                    ? i.UserData!.Name
                    : Character.GetDefaultFromTypeId(i.TypeId).Name,
                Number = i.Number,

                RarityNum = (int)Character.GetDefaultFromTypeId(i.TypeId).Rarity
            })
            .ToArrayAsync();

        return Ok(gottenCollection);
    }
}