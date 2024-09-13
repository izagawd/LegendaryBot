using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BasicFunctionality;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PublicInfo;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Entities.BattleEntities.Blessings;

public class BlessingDatabaseConfiguration : IEntityTypeConfiguration<Blessing>
{
    public void Configure(EntityTypeBuilder<Blessing> builder)
    {
        builder.HasKey(i => i.Id);
        var starting = builder.HasDiscriminator(i => i.TypeId);
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Blessing>())
            starting = starting.HasValue(i.GetType(), i.TypeId);
    }
}

public abstract class Blessing : IInventoryEntity, IGuidPrimaryIdHaver
{
    private static readonly Dictionary<int, Blessing> _cachedDefaultBlessingsTypeIds = [];
 


    [Timestamp] public uint Version { get; private set; }

    public virtual bool SpawnsNormally => true;
    public long? CharacterId { get; set; }


    [NotMapped] public virtual int Attack => 20 + LevelMilestone * 40;
    [NotMapped] public virtual int Health => 70 + LevelMilestone * 80;
    public int LevelMilestone => Character?.LevelMilestone ?? 0;
    public bool IsInStandardBanner => true;
    public CharacterPartials_Character? Character { get; set; }



    public long Id { get; set; }
    public abstract string Description { get; }
    public abstract Rarity Rarity { get; }


    public UserData? UserData { get; set; }

    public bool CanBeTraded => true;


    public abstract int TypeId { get; protected init; }
    public Type TypeGroup => typeof(Blessing);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public long UserDataId { get; set; }
    public virtual string ImageUrl => $"{Information.ApiDomainName}/battle_images/blessings/{GetType().Name}.png";
    public abstract string Name { get; }

    public static Blessing GetDefaultFromTypeId(int typeId)
    {
        if (!_cachedDefaultBlessingsTypeIds.TryGetValue(typeId, out var blessing))
        {
            blessing = TypesFunction.GetDefaultObjectsAndSubclasses<Blessing>()
                .FirstOrDefault(i => i.TypeId == typeId);
            if (blessing is null) throw new Exception($"Blessing with type id {typeId} not found");

            _cachedDefaultBlessingsTypeIds[typeId] = blessing;
        }

        return blessing;
    }

    public static Blessing GetRandomBlessing(Dictionary<Rarity, double> rates)
    {
        var groups = TypesFunction
            .GetDefaultObjectsAndSubclasses<Blessing>()
            .Where(i => i.SpawnsNormally)
            .GroupBy(i => i.Rarity)
            .ToImmutableArray();

        var rarityToUse = BasicFunctions.GetRandom(rates);
        var randomBlessing
            = BasicFunctions.RandomChoice(
                groups.First(i => i.Key == rarityToUse).Select(i => i)).GetType();
        return (Blessing)Activator.CreateInstance(randomBlessing)!;
    }

 
}