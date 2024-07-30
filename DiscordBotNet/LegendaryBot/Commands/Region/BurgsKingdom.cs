using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Commands;

public class BurgsKingdom : Region
{
    public override string Name => "Burgs Kingdom";

    public override IEnumerable<Type> ObtainableCharacters =>
    [
        typeof(RoyalKnight),
        typeof(Blast), typeof(Slime), typeof(Thug), typeof(Thana), typeof(Cheerleader)
    ];
}