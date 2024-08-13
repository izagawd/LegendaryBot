using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Weapon : Gear
{
    public override string Name => "Weapon";

    public override int TypeId
    {
        get => 6;
        protected init {}
    }

  
    public sealed override IEnumerable<Type> PossibleMainStats =>
    [
        GearStat.AttackFlatType,
       
       
    ];
}