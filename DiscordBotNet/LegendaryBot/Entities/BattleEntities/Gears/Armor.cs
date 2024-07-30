using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Armor : Gear
{
    public override string Name => "Armor";

    public Armor()
    {
        TypeId = 2;
    }

    public sealed override IEnumerable<Type> PossibleMainStats => [GearStat.DefenseFlatType];
}