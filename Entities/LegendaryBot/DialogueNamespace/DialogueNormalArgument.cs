namespace Entities.LegendaryBot.DialogueNamespace;

public class DialogueNormalArgument : DialogueArgument
{
    /// <summary>
    /// The amount of text dialogue the user would make
    /// </summary>
    public required IEnumerable<string> DialogueTexts { get; init; }
}