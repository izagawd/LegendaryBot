using DSharpPlus.Entities;

namespace Entities.LegendaryBot.Results;

public class DialogueResult
{
    /// <summary>
    ///     Not null if a dialogue decision was involved. It is the id of the returned decision
    /// </summary>
    public string? Decision { get; init; }

    public bool TimedOut { get; init; } = false;
    public required DiscordMessage Message { get; init; }
    public bool Skipped { get; init; } = false;
}