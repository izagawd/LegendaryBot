using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Ring : Gear
{
    public override string Name => "Ring";

    public override int TypeId
    {
        get => 5;
        protected init { }
    }

    public sealed override IEnumerable<Type> PossibleMainStats =>
    [
        GearStat.AttackPercentageType,
        GearStat.HealthPercentageType,
        GearStat.ResistanceType,
        GearStat.EffectivenessType,
        GearStat.DefensePercentageType
    ];
}