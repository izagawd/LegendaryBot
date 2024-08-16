using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Helmet : Gear
{
    public override string Name => "Helmet";

    public override int TypeId
    {
        get => 3;
        protected init { }
    }


    public sealed override IEnumerable<Type> PossibleMainStats => [GearStat.HealthFlatType];
}