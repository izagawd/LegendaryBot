using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public  class Boots : Gear
{
    public override string Name => "Boots";

    public Boots()
    {
        TypeId = 1;
    }

    public sealed override IEnumerable<Type> PossibleMainStats =>
    [
        GearStat.AttackPercentageType,
        GearStat.HealthPercentageType,
        GearStat.SpeedFlatType,
        GearStat.DefensePercentageType,
       
    ];
}