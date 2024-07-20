using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Armor : Gear
{
    public override int TypeId => 2;
    public sealed override IEnumerable<Type> PossibleMainStats => [GearStat.DefenseFlatType];
}