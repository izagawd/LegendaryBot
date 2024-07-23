using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Helmet : Gear
{
    public Helmet()
    {
        TypeId = 3;
    }
    

    public sealed override IEnumerable<Type> PossibleMainStats => [GearStat.HealthFlatType];
}