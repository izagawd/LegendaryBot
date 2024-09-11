using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Characters;

namespace WebsiteApi.Apis;

[Route("[controller]")]
[ApiController]
public class CharacterInfoPageController : ControllerBase
{
    private PostgreSqlContext Context;


    public CharacterInfoPageController(PostgreSqlContext context)
    {
        Context = context;
    }

    private static readonly int PlayerTypeId = TypesFunction.GetDefaultObject<Player>().TypeId;

    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult> GetTeamDataAsync([FromForm] int characterNumber)
    {
        var userDataId = User.GetDiscordUserId();
        var gotten = await Context.Set<UserData>().Where(i => i.DiscordId == userDataId)
            .Select(i => new CharacterInfo.CharacterInfoDto()
            {
                AllGears = i.Gears.Select(j => new CharacterInfo.GearDto()
                {
                    GearName = Gear.GetDefaultFromTypeId(j.TypeId).Name,
                    GearStats = j.Substats.Select(k => new CharacterInfo.GearStatDto()
                    {
                        Value = k.Value,
                        IsMainStat = k.IsMainStat != null,
                        IsPercentage = GearStat.GetDefaultFromTypeId(k.TypeId).IsPercentage
                    }).ToArray(),
                    TypeId = j.TypeId,
                    Id = j.Id,
                    ImageUrl = Gear.GetDefaultFromTypeId(j.TypeId).ImageUrl,
                    OriginalOwnerImageUrl = j.Character != null ? 
                        (j.Character.TypeId != PlayerTypeId ?  Character.GetDefaultFromTypeId(j.Character.TypeId).ImageUrl
                            : Player.GetImageUrl(i.Gender)) : null

                }).ToArray(),
                CharacterDto = i.Characters.Where(j => j.Number == characterNumber)
                    .Select(j => new CharacterInfo.CharacterDto()
                    {
                        Id = j.Id,
                        ImageUrl = j.TypeId != PlayerTypeId
                            ? Character.GetDefaultFromTypeId(j.TypeId).ImageUrl
                            : Player.GetImageUrl(i.Gender),
                        Name = j.TypeId != PlayerTypeId
                            ? Character.GetDefaultFromTypeId(j.TypeId).Name
                            : i.Name,
                        Level = j.Level
                    }).FirstOrDefault()!,
                CharacterEquippedGearsId = i.Characters.Where(j => j.Number == characterNumber)
                    .SelectMany(j => j.Gears).Select(j => j.Id).ToList()
            }).FirstOrDefaultAsync();
        if (gotten is null)
        {
            return BadRequest("Your data was not found in database");
        }

        if (gotten.CharacterDto is null)
        {
            return BadRequest($"Character with number {characterNumber} not found");
        }

        return Ok(gotten);
    }
}