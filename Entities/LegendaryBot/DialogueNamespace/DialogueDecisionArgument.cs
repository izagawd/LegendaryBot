using DSharpPlus.Entities;

namespace Entities.LegendaryBot.DialogueNamespace;

public class DialogueDecisionArgument : DialogueArgument
{
    public required string DialogueText { get; init; }
    /// <summary>
    /// The action rows that would be used to let the user make a decision
    /// </summary>
    public required IEnumerable<DiscordActionRowComponent> ActionRows { get; init; }
}