using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public abstract class Necklace : Gear
{
    public sealed override IEnumerable<Type> PossibleMainStats =>
    [
        GearStat.AttackPercentageType,
        GearStat.HealthPercentageType,
        GearStat.CriticalChanceType,
        GearStat.CriticalDamageType,
        GearStat.DefensePercentageType,
    ];
}