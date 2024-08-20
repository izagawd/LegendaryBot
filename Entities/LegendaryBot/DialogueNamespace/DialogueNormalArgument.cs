namespace Entities.LegendaryBot.DialogueNamespace;

public class DialogueNormalArgument : DialogueArgument
{
    public required IEnumerable<string> DialogueTexts { get; init; }
}