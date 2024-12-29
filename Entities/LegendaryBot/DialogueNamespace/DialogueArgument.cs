using DSharpPlus.Entities;

namespace Entities.LegendaryBot.DialogueNamespace;

public abstract class DialogueArgument
{
    public required DialogueProfile DialogueProfile { get; init; }
    public DiscordColor CharacterColor => DialogueProfile.CharacterColor;

    
    public string CharacterName => DialogueProfile.CharacterName;

    public string CharacterUrl => DialogueProfile.CharacterUrl;
}