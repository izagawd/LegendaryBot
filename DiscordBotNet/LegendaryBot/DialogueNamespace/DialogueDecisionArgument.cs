using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.DialogueNamespace;

public class DialogueDecisionArgument : DialogueArgument
{
    public required string DialogueText { get; init; }
    public required IEnumerable<DiscordActionRowComponent> ActionRows { get; init; }
}