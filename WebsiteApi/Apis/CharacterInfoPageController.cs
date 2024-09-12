using System.Linq.Expressions;
using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot;
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
using Website.Pages.Teams;

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
        Expression<Func<UserData, IEnumerable<Character>>> zaExpr = i =>
            i.Characters.Where(j => j.Number == characterNumber);
        var userDataId = User.GetDiscordUserId();
        var gotten = await Context.Set<UserData>()
            .AsNoTrackingWithIdentityResolution()
            .Where(i => i.DiscordId == userDataId)
            .Include(i => i.Characters.Where(j => j.Number == characterNumber))
            .Include(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .Include(i => i.Blessings)
            .FirstOrDefaultAsync();
        
        
        if (gotten is null)
        {
            return BadRequest("Your data was not found in database");
        }

        var character = gotten.Characters.FirstOrDefault(i => i.Number == characterNumber);
        
        if (character is null)
        {
            return BadRequest($"Character with number {characterNumber} not found");
        }

        var dto = new CharacterInfo.CharacterInfoDto();
        dto.CharacterDto = new CharacterInfo.CharacterDto
        {
            Id = character.Id,
            ImageUrl = character.ImageUrl,
            Level = character.Level,
            Name = character.Name,
            Number = character.Number,

        };
        var theDic = dto.CharacterDto.TheEquippedOnes = new();
        foreach (var i in character.Gears)
        {
            theDic[GetWorkingWithValue(i.TypeId)] = i.Id;
        }

        dto.AllGears = gotten.Gears.Select(j => new CharacterInfo.GearDto()
        {
            Id = j.Id,
            GearName = j.Name,
            ImageUrl = j.ImageUrl,
            RarityName = j.Rarity.ToString(),
            RarityNum = (int) j.Rarity,
            GearStats = j.Stats.Select(k => new CharacterInfo.GearStatDto()
            {
                IsMainStat = k.IsMainStat is not null,
                IsPercentage = k.IsPercentage,
                StatName = k.StatType.GetShortName(),
                Value = k.Value,
                
            }).ToArray(),
            TypeId = j.TypeId,
            
        }).ToArray();
        dto.AllBlessings = gotten.Blessings.Select(i => new CharacterInfo.BlessingDto()
        {
            Id = i.Id,
            ImageUrl = i.ImageUrl,
            Name = i.Name,
            TypeId = i.TypeId
        }).ToArray();
        theDic[CharacterInfo.WorkingWith.Blessing] = character.Blessing?.Id;
        character.LoadStats();

        dto.CharacterDto.CharacterStatsString =
            Enum.GetValues<StatType>().Select(i => $"{i.GetShortName()}: {character.GetStatFromType(i)}").ToArray();
        dto.WorkingWithToTypeIdHelper = Helper;
    
        return Ok(gotten);
    }

    private static readonly Dictionary<CharacterInfo.WorkingWith, int> Helper = TypesFunction
        .GetDefaultObjectsAndSubclasses<Gear>()
        .ToDictionary(i => GetWorkingWithValue(i.TypeId), i => i.TypeId);
}