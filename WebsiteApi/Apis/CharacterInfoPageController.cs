using System.Collections.Immutable;
using System.Diagnostics;
using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
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
    private readonly PostgreSqlContext postgreSqlContext;

    public CharacterInfoPageController(PostgreSqlContext postgreSqlContext)
    {
        this.postgreSqlContext = postgreSqlContext;
    }


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
    [HttpPost("remove-blessing")]
    public async Task<IActionResult> RemoveBlessingAsync([FromForm] long characterId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await postgreSqlContext.Set<UserData>()
            .Include(i => i.Characters.Where(j => j.Id == characterId))
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Characters)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);
        if (userData is null) return BadRequest("Userdata not found");
        if (userData.IsOccupied) return BadRequest("You are occupied");

        var character = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (character is null) return BadRequest("Character not found");

        character.Blessing = null;
        await postgreSqlContext.SaveChangesAsync();
        return Ok(GenerateStatsStrings(character));
    }

    [Authorize]
    [HttpPost("equip-blessing")]
    public async Task<IActionResult> EquipBlessingAsync([FromForm] long characterId, [FromForm] int blessingTypeId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await postgreSqlContext.Set<UserData>()
            .Include(i => i.Characters.Where(j => j.Id == characterId))
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Characters)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .Include(i => i.Blessings.Where(j => j.TypeId == blessingTypeId && j.CharacterId == null))
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);

        if (userData is null) return BadRequest("Userdata not found");
        if (userData.IsOccupied) return BadRequest("You are occupied");
        var blessing = userData.Blessings.FirstOrDefault(j => j.TypeId == blessingTypeId && j.CharacterId == null);
        if (blessing is null)
            return BadRequest(
                $"you have no free blessing with name {Blessing.GetDefaultFromTypeId(blessingTypeId).Name}");

        var character = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (character is null) return BadRequest("Something went wrong");

        character.Blessing = blessing;
        await postgreSqlContext.SaveChangesAsync();
        return Ok(GenerateStatsStrings(character));
    }

    [Authorize]
    [HttpPost("equip-gear")]
    public async Task<IActionResult> EquipGearAsync([FromForm] long characterId, [FromForm] long gearId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await postgreSqlContext.Set<UserData>()
            .Include(i => i.Characters.Where(j => j.Id == characterId))
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .Include(i => i.Characters)
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Gears.Where(j => j.Id == gearId))
            .ThenInclude(i => i.Character)
            .Include(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);
        if (userData is null)
            return BadRequest("User data not found");
        if (userData.IsOccupied) return BadRequest("You are occupied");
        var gottenGear = userData.Gears.FirstOrDefault(i => i.Id == gearId);
        if (gottenGear is null) return BadRequest("Something went wrong");

        var character = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (character is null) return BadRequest("Something went wrong");
        character.Gears.RemoveAll(i => i.GetType() == gottenGear.GetType());
        character.Gears.Add(gottenGear);
        await postgreSqlContext.SaveChangesAsync();

        return Ok(GenerateStatsStrings(character));
    }

    [Authorize]
    [HttpPost("remove-gear")]
    public async Task<IActionResult> RemoveGearAsync([FromForm] long characterId, [FromForm] int gearTypeId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await postgreSqlContext.Set<UserData>()
            .Include(i => i.Characters.Where(j => j.Id == characterId))
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Characters)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);

        if (userData is null) return BadRequest("user data not found");

        if (userData.IsOccupied) return BadRequest("You are occupied");

        var character = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (character is null) return BadRequest("Something went wrong");
        character.Gears.RemoveAll(i => i.TypeId == gearTypeId);
        await postgreSqlContext.SaveChangesAsync();
        return Ok(GenerateStatsStrings(character));
    }

    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult> GetCharacterDataAsync([FromQuery] int characterTypeId)
    {
        var typeIdToLookFor = characterTypeId;
        var userDataId = User.GetDiscordUserId();
    
        var gotten = await postgreSqlContext.Set<UserData>()
            .AsNoTrackingWithIdentityResolution()
            .Include(i => i.Characters.Where(j => j.TypeId == typeIdToLookFor))
            .Include(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .Include(i => i.Gears)
            .ThenInclude(i => i.Character)
            .Include(i => i.Blessings)
            .FirstOrDefaultAsync(i => i.DiscordId == userDataId);

        if (gotten is null) return BadRequest("Your data was not found in database");
        if (gotten.IsOccupied) return BadRequest("You are occupied");
        var character = gotten.Characters
            .FirstOrDefault(i => i.TypeId == typeIdToLookFor);

        if (character is null) return BadRequest($"Character with type id {typeIdToLookFor} not found");

        var dto = new CharacterInfo.CharacterInfoDto();
        dto.CharacterDto = new CharacterInfo.CharacterDto
        {
            RarityNum = (int)character.Rarity,
            Id = character.Id,
            TypeId = character.TypeId,
            Level = character.Level,
            Name = character.Name,

        };
        var theDic = dto.CharacterDto.TheEquippedOnes = new Dictionary<CharacterInfo.WorkingWith, long?>();
        foreach (var i in character.Gears) theDic[GetWorkingWithValue(i.TypeId)] = i.Id;

        dto.AllGears = gotten.Gears.Select(j => new CharacterInfo.GearDto
        {
            Id = j.Id,

            RarityNum = (int)j.Rarity,
            Number = j.Number,
            GearStats = j.Stats.Select(k => new CharacterInfo.GearStatDto
            {
                IsMainStat = k.IsMainStat is not null,
                IsPercentage = k.IsPercentage,
                TypeId = k.TypeId,
                Value = k.Value
            }).ToArray(),
            EquippedCharacterTypeId = j.Character?.TypeId,
            TypeId = j.TypeId
        }).ToArray();
        dto.AllBlessings = gotten.Blessings.GroupBy(i => i.TypeId)
            .Select(i => new CharacterInfo.BlessingDto
            {
                TypeId = i.Key,
                RemainingStacks = i.Count(j => j.CharacterId is null),
                Stacks = i.Count(),
                RarityNum = (int)i.First().Rarity,
                Description = i.First().Description,
                Name = i.First().Name
            }).ToArray();
        theDic[CharacterInfo.WorkingWith.Blessing] = character.Blessing?.TypeId;


        dto.CharacterDto.CharacterStatsString =
            GenerateStatsStrings(character);


        dto.GearStatNameByTypeId = TypesFunction.GetDefaultObjectsAndSubclasses<GearStat>()
            .DistinctBy(i => i.TypeId).ToImmutableDictionary(i => i.TypeId, i => i.StatType.GetShortName());
        dto.GearNameByTypeId =
            TypesFunction.GetDefaultObjectsAndSubclasses<Gear>().DistinctBy(i => i.TypeId)
                .ToImmutableDictionary(i => i.TypeId, i => i.Name);
        dto.WorkingWithToTypeIdHelper = TypesFunction
            .GetDefaultObjectsAndSubclasses<Gear>()
            .ToDictionary(i => GetWorkingWithValue(i.TypeId), i => i.TypeId);
        ;

        return Ok(dto);
    }

    public static string[] GenerateStatsStrings(Character character)
    {
        character.LoadStats();
        return Enum.GetValues<StatType>().Select(i =>
        {
            var perc = i.IsAPercentageType() ? "%" : "";
            return $"{i.GetShortName()}: {character.GetStatFromType(i)}{perc}";
        }).ToArray();
    }
}