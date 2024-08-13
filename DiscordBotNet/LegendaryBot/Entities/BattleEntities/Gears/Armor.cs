using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Armor : Gear
{
    public override string Name => "Armor";

    public override int TypeId
    {
        get => 2;
        protected init {}
    }


    public sealed override IEnumerable<Type> PossibleMainStats => [GearStat.DefenseFlatType];
}