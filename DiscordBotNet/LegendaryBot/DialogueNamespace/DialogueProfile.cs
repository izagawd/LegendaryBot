using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.DialogueNamespace;

public class DialogueProfile
{
    public required DiscordColor CharacterColor { get; init; } = DiscordColor.Green;

    public required string CharacterName { get; init; }

    public required string CharacterUrl { get; init; }
}