using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Necklace : Gear
{
    public override int TypeId => 4;
    public sealed override IEnumerable<Type> PossibleMainStats =>
    [
        GearStat.AttackPercentageType,
        GearStat.HealthPercentageType,
        GearStat.CriticalChanceType,
        GearStat.CriticalDamageType,
        GearStat.DefensePercentageType,
    ];
}