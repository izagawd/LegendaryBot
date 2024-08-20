using Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace Entities.LegendaryBot.Entities.BattleEntities.Gears;

public class Necklace : Gear
{
    public override string Name => "Necklace";

    public override int TypeId
    {
        get => 4;
        protected init { }
    }

    public sealed override IEnumerable<Type> PossibleMainStats =>
    [
        GearStat.AttackPercentageType,
        GearStat.HealthPercentageType,
        GearStat.CriticalChanceType,
        GearStat.CriticalDamageType,
        GearStat.DefensePercentageType
    ];
}