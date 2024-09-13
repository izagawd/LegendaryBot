using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Characters;

namespace WebsiteApi.Apis;

[Route("[controller]")]
[ApiController]
public class CharactersPageController(PostgreSqlContext post) : ControllerBase
{
    private static readonly int PlayerTypeId = TypesFunction.GetDefaultObject<Player>().TypeId;

    [HttpGet("get-characters")]
    [Authorize]
    public async Task<ActionResult<Characters.CharacterDto[]>> GetCharactersAsync()
    {
        var zaId = User.GetDiscordUserId();
        var userData = await post.Set<UserData>()
            .Include(i => i.Characters)
            .FirstOrDefaultAsync(i => i.DiscordId == zaId);
            
            
            var toDto = userData.Characters
            .Select(i => new Characters.CharacterDto
            {
                Name = i.Name,
                Level = i.Level,
                TypeId = i.TypeId,
                Number = i.Number,
                RarityNum = (int)Character.GetDefaultFromTypeId(i.TypeId).Rarity
            })
            .ToArray();

        return Ok(toDto);
    }
}