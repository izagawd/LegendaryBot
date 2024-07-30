using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Commands;

public class RogueHaven : Region
{
    public override string Name => "Rogue Haven";

    public override IEnumerable<Type> ObtainableCharacters =>
    [
        typeof(Thug),
        typeof(Delinquent), typeof(Police), typeof(Roxy), typeof(Slime),
        typeof(Takeshi), typeof(CommanderJean)
    ];
}