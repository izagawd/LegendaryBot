using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Commands;

public class RogueHaven : Region
{
    public override IEnumerable<Type> ObtainableCharacters =>
    [
        typeof(Thug),
        typeof(Delinquent), typeof(Police), typeof(Roxy)
    ];
}