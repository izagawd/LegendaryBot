using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Ring : Gear
{
    public override int TypeId => 5;
    public sealed override IEnumerable<Type> PossibleMainStats =>[
    
        GearStat.AttackPercentageType,
        GearStat.HealthPercentageType,
        GearStat.ResistanceType,
        GearStat.EffectivenessType,
        GearStat.DefensePercentageType,
       
    ];
}