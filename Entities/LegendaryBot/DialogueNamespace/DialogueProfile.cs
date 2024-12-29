using DSharpPlus.Entities;

namespace Entities.LegendaryBot.DialogueNamespace;

public class DialogueProfile
{
    /// <summary>
    /// The color that would e used for the embed for the dialogue
    /// </summary>
    public required DiscordColor CharacterColor { get; init; } = DiscordColor.Green;
    public required string CharacterName { get; init; }

    public required string CharacterUrl { get; init; }
}