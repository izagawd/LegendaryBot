using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website;
using Website.Pages.Characters;

namespace WebsiteApi.Apis.Hubs;


[Authorize]
public class EquipmentHub : Hub
{



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


    public ClaimsPrincipal User => Context.User!;

    public async Task<HubResult<string[]>> RemoveBlessingAsync(long characterId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await postgreSqlContext.Set<UserData>()
            .Include(i => i.Characters.Where(j => j.Id == characterId))
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Characters)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
     
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);
        if (userData is null)
        {
            return  HubResult<string[]>.Failure("Userdata not found");
        }
        if (userData.IsOccupied)
        {
            return HubResult<string[]>.Failure("You are occupied");
        }

        var character = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (character is null)
        {
            return HubResult<string[]>.Failure("Character not found");
        }

        character.Blessing = null;
        await postgreSqlContext.SaveChangesAsync();
        return GenerateStatsStrings(character);
    }
     public async Task<HubResult<string[]>> EquipBlessingAsync([FromForm] long characterId, [FromForm] int blessingTypeId)
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

        if (userData is null)
        {
            return  HubResult<string[]>.Failure("Userdata not found");
        }
        if (userData.IsOccupied)
        {
            return HubResult<string[]>.Failure("You are occupied");
        }
        var blessing = userData.Blessings.FirstOrDefault(j => j.TypeId == blessingTypeId && j.CharacterId == null);
        if (blessing is null)
        {
            return HubResult<string[]>.Failure(
                $"you have no free blessing with name {Blessing.GetDefaultFromTypeId(blessingTypeId).Name}");
        }

        var character = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (character is null)
        {
            return HubResult<string[]>.Failure($"Something went wrong");
        }

        character.Blessing = blessing;
        await postgreSqlContext.SaveChangesAsync();
        return GenerateStatsStrings(character);
    }

    public async Task<HubResult<string[]>> EquipGearAsync([FromForm] long characterId, [FromForm] long gearId)
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
            return HubResult<string[]>.Failure("User data not found");
        if (userData.IsOccupied)
        {
            return HubResult<string[]>.Failure("You are occupied");
        }
        var gottenGear = userData.Gears.FirstOrDefault(i => i.Id == gearId);
        if (gottenGear is null)
        {
            return HubResult<string[]>.Failure("Something went wrong");
        }

        var character = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (character is null)
        {
            return HubResult<string[]>.Failure("Something went wrong");
        }
        character.Gears.RemoveAll(i => i.GetType() == gottenGear.GetType());
        character.Gears.Add(gottenGear);
        await postgreSqlContext.SaveChangesAsync();
        
        return  GenerateStatsStrings(character);
    }

    private PostgreSqlContext postgreSqlContext;

    public EquipmentHub(PostgreSqlContext postgreSqlContext)
    {
        this.postgreSqlContext = postgreSqlContext;
        "CREATED".Print();
    }

  
    public async Task<HubResult<string[]>> RemoveGearAsync([FromForm] long characterId, [FromForm] int gearTypeId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await postgreSqlContext.Set<UserData>()
            .Include(i => i.Characters.Where(j => j.Id == characterId))
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Characters)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);

        
        
        
        if (userData is null)
        {
            return HubResult<string[]>.Failure("user data not found");
        }
        await postgreSqlContext.Entry(userData).ReloadAsync();
        if (userData.IsOccupied)
        {
            return HubResult<string[]>.Failure("You are occupied");
        }

        var character = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (character is null)
        {
            return HubResult<string[]>.Failure("Something went wrong");
        }
        character.Gears.RemoveAll(i => i.TypeId == gearTypeId);
        await postgreSqlContext.SaveChangesAsync();
        return (GenerateStatsStrings(character));
    }


    public async Task<HubResult<CharacterInfo.CharacterInfoDto>> GetEquipmentDataAsync([FromQuery] int characterNumber)
    {
     
        var userDataId = User.GetDiscordUserId();
        var gotten = await postgreSqlContext.Set<UserData>()
            .AsNoTrackingWithIdentityResolution()
            .Where(i => i.DiscordId == userDataId)
            .Include(i => i.Characters.Where(j => j.Number == characterNumber))
            .Include(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .Include(i => i.Blessings)
            .FirstOrDefaultAsync();
        
        
        if (gotten is null)
        {
            
            return HubResult<CharacterInfo.CharacterInfoDto>.Failure("Character not found");
        }
        if (gotten.IsOccupied)
        {
            return HubResult<CharacterInfo.CharacterInfoDto>.Failure("You are occupied");
        }
 
        var character = gotten.Characters.FirstOrDefault(i => i.Number == characterNumber);
        
        if (character is null)
        {
            return HubResult<CharacterInfo.CharacterInfoDto>.Failure($"Character with number {characterNumber} not found");
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
            Number = j.Number,
            GearStats = j.Stats.Select(k => new CharacterInfo.GearStatDto()
            {
                IsMainStat = k.IsMainStat is not null,
                IsPercentage = k.IsPercentage,
                StatName = k.StatType.GetShortName(),
                Value = k.Value,
                
            }).ToArray(),
            TypeId = j.TypeId,
            
        }).ToArray();
        dto.AllBlessings = gotten.Blessings.GroupBy(i => i.TypeId)
            .Select(i => new CharacterInfo.BlessingDto()
        {
   
            ImageUrl = i.First().ImageUrl,
            Name = i.First().Name,
            TypeId = i.Key,
            RemainingStacks = i.Count(j => j.CharacterId is null),
            Stacks = i.Count(),
            RarityName = i.First().Rarity.ToString(),
            Description = i.First().Description
        }).ToArray();
        theDic[CharacterInfo.WorkingWith.Blessing] = character.Blessing?.TypeId;


        dto.CharacterDto.CharacterStatsString =
            GenerateStatsStrings(character);
        dto.WorkingWithToTypeIdHelper = Helper;


        return dto;
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
    private static readonly Dictionary<CharacterInfo.WorkingWith, int> Helper = TypesFunction
        .GetDefaultObjectsAndSubclasses<Gear>()
        .ToDictionary(i => GetWorkingWithValue(i.TypeId), i => i.TypeId);
}