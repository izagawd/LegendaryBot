using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BasicFunctionality;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PublicInfo;
using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

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
    private static readonly ConcurrentDictionary<int, Blessing> _cachedDefaultBlessingsTypeIds = [];


    [Timestamp] public uint Version { get; private set; }

    public virtual bool SpawnsNormally => true;
    public long? CharacterId { get; set; }


    [NotMapped] public virtual int Attack => 20 + (CharacterLevel / 10) * 40;
    [NotMapped] public virtual int Health => 70 + (CharacterLevel / 10) * 80;
    public int CharacterLevel => Character?.Level ?? 0;
    public virtual bool IsInStandardBanner => true;
    public Character? Character { get; set; }


    public long Id { get; set; }
    public abstract string Description { get; }
    public abstract Rarity Rarity { get; }


    public UserData? UserData { get; set; }

    


    public abstract int TypeId { get; protected init; }
    public Type TypeGroup => typeof(Blessing);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public long UserDataId { get; set; }
    public string ImageUrl => $"{Information.BlessingImagesDirectory}/{TypeId}.png";
    public abstract string Name { get; }

    public static Blessing GetDefaultFromTypeId(int typeId)
    {
        return _cachedDefaultBlessingsTypeIds.GetOrAdd(typeId,
            i => TypesFunction.GetDefaultObjectsAndSubclasses<Blessing>()
                     .FirstOrDefault(j => j.TypeId == i) ??
                 throw new Exception($"Blessing with type id {i} not found"));

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