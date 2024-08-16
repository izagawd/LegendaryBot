namespace DiscordBotNet.LegendaryBot.Results;

public class ExperienceGainResult
{
    public int ExcessExperience { get; set; }
    public string Text { get; init; } = "";

    public override string ToString()
    {
        return Text;
    }
}