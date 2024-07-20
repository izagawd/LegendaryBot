using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Helmet : Gear
{
    public override int TypeId => 3;
    public sealed override IEnumerable<Type> PossibleMainStats => [GearStat.HealthFlatType];
}