using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class ArtifactStatConfiguration : IEntityTypeConfiguration<GearStat>
{
    public void Configure(EntityTypeBuilder<GearStat> builder)
    {
        builder.HasKey(i => i.Id);
    }
}

public abstract class GearStat
{
    [NotMapped]
    public static Type AttackPercentageType { get; } = typeof(AttackPercentageGearStat);
    [NotMapped]  public static Type AttackFlatType { get; } = typeof(AttackFlatGearStat);
    [NotMapped] public static Type HealthPercentageType { get; } = typeof(HealthPercentageGearStat);
    [NotMapped]public static Type HealthFlatType { get; } = typeof(HealthFlatGearStat);
    [NotMapped] public static Type DefenseFlatType { get; } = typeof(DefenseFlatGearStat);
    [NotMapped]  public static Type DefensePercentageType { get; } = typeof(DefensePercentageGearStat);
    [NotMapped] public static Type CriticalChanceType { get; } = typeof(CriticalChanceGearStat);
    [NotMapped]  public static Type CriticalDamageType { get; } = typeof(CriticalDamageGearStat);
    [NotMapped]  public static Type ResistanceType { get; } = typeof(ResistanceGearStat);
    [NotMapped]  public static Type EffectivenessType { get; } = typeof(EffectivenessGearStat);
    [NotMapped] public static Type SpeedFlatType { get; } = typeof(SpeedFlatGearStat);
    [NotMapped]  public static Type SpeedPercentageType { get; } = typeof(SpeedPercentageGearStat);

    /// <summary>
    /// This is called when the gear that owns this stat is loaded. It sets the main stat's value according to
    /// the rarity and the level of the gear
    /// </summary>
    /// <param name="rarity"></param>
    /// <param name="level"></param>
    public  void SetMainStatValue(Rarity rarity, int level)
    {
        Value = GetMainStatValue(rarity, level);
    }

    
    public Gear Gear { get; set; }
    public Guid Id { get; set; }
    public Guid GearId { get; set; }
    
    /// <summary>
    /// if not null them this is the main stat for a gear
    /// </summary>
    public Guid? IsMainStat { get; set; }
    
    
    
    public abstract int GetMainStatValue(Rarity rarity, int level);
/// <summary>
/// the amount of times a substat has been increased, if this GearStat is a substat
/// </summary>
    public int TimesIncreased { get; private set; }
    /// <summary>
    /// the value of the stat
    /// </summary>
    public float Value { get; protected set; }
    
    
    
    public abstract void AddStats(Character character);

 
    [NotMapped]
    public static IEnumerable<Type> AllGearStatTypes { get; }

    static GearStat()
    {
        AllGearStatTypes = DefaultObjects.AllAssemblyTypes.Where(i => !i.IsAbstract && i.IsSubclassOf(typeof(GearStat))).ToImmutableArray();
    }
    
    public override string ToString()
    {
        return $"{BasicFunctionality.Englishify(GetType().Name.Replace("GearStat",""))}: {Value}";
    }
    /// <summary>
    /// Increases the value of the substat by a random amount between GetMaximumSubstatLevelIncrease
    /// and GetMinimum
    /// </summary>
    /// <param name="rarity"></param>
    public void Increase(Rarity rarity)
    {
        TimesIncreased += 1;
        Value += BasicFunctionality.GetRandomNumberInBetween(GetMinimumSubstatLevelIncrease(rarity),
            GetMaximumSubstatLevelIncrease(rarity));
    }
    /// <summary>
    /// Gets the maximum amount this stat can increase depending on the rarity of the gear
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public abstract int GetMaximumSubstatLevelIncrease(Rarity rarity);
    /// <summary>
    /// Gets the minimum amount this stat can increase depending on the rarity of the gear
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public abstract int GetMinimumSubstatLevelIncrease(Rarity rarity);
}