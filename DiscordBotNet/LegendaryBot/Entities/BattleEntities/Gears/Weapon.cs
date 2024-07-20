using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Weapon : Gear
{

    public override int TypeId => 6;
    public sealed override IEnumerable<Type> PossibleMainStats =>
    [
        GearStat.AttackFlatType,
       
       
    ];
}