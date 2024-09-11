using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using Entities.LegendaryBot.Entities.Items;
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


    public static CharacterInfo.WorkingWith GetWorkingWithValue(int typeId)
    {
        var defaultd = Gear.GetDefaultFromTypeId(typeId);
        switch (defaultd)
        {
            case Armor:
                return CharacterInfo.WorkingWith.Armor;
            case Necklace:
                return CharacterInfo.WorkingWith.Necklace;
            case Helmet:
                return CharacterInfo.WorkingWith.Helmet;
            case Weapon:
                return CharacterInfo.WorkingWith.Weapon;
            case Ring:
                return CharacterInfo.WorkingWith.Ring;
            case Boots:
                return CharacterInfo.WorkingWith.Boots;
            default:
                throw new ArgumentException("Invalid typeId", nameof(typeId));
        }
    }
    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult> GetTeamDataAsync([FromQuery] int characterNumber)
    {
        var userDataId = User.GetDiscordUserId();
        var gotten = await Context.Set<UserData>().Where(i => i.DiscordId == userDataId)
            .Select(i =>new{EquippedStuff =
                 i.Characters.Where(j => j.Number == characterNumber)
                    .Select(j => new{GearData = j.Gears.Select(k => new{k.TypeId, k.Id}).ToArray(),
                        EquippedBlessingId = j.Blessing != null ? j.Blessing.Id : (long?)null }).FirstOrDefault()
                , TheData = new CharacterInfo.CharacterInfoDto()
            {
                AllGears = i.Gears.Select(j => new CharacterInfo.GearDto()
                {
                    GearName = Gear.GetDefaultFromTypeId(j.TypeId).Name,
                    GearStats = j.Stats.Select(k => new CharacterInfo.GearStatDto
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
                            : Player.GetImageUrl(i.Gender)) : null,
                    
                    

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
                        Level = j.Level,
                        Number = j.Number,
                    }).FirstOrDefault()!,
                AllBlessings = i.Blessings.Select(j => new CharacterInfo.BlessingDto()
                {
                    Id = j.Id,
                    ImageUrl = Blessing.GetDefaultFromTypeId(j.TypeId).ImageUrl,
                    Name = Blessing.GetDefaultFromTypeId(j.TypeId).Name,
                    TypeId = j.TypeId
                    
                }).ToArray()
                }
            
            }).FirstOrDefaultAsync();
        if (gotten is null)
        {
            return BadRequest("Your data was not found in database");
        }

        if (gotten.TheData.CharacterDto is null)
        {
            return BadRequest($"Character with number {characterNumber} not found");
        }

        var theDic = gotten.TheData.CharacterDto.TheEquippedOnes = new();

        foreach (var i in gotten.EquippedStuff!.GearData)
        {
            theDic[GetWorkingWithValue(i.TypeId)] = i.Id;
        }

        theDic[CharacterInfo.WorkingWith.Blessing] = gotten.EquippedStuff.EquippedBlessingId;
        return Ok(gotten.TheData);
    }
}