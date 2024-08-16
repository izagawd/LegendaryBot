using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Boots : Gear
{
    public override string Name => "Boots";

    public override int TypeId
    {
        get => 1;
        protected init { }
    }


    public sealed override IEnumerable<Type> PossibleMainStats =>
    [
        GearStat.AttackPercentageType,
        GearStat.HealthPercentageType,
        GearStat.SpeedFlatType,
        GearStat.DefensePercentageType
    ];
}