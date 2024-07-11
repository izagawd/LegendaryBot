namespace DiscordBotNet.LegendaryBot.Results;

public class ExperienceGainResult
{

    public long ExcessExperience { get; set; }
    public string Text { get; init; } = "";
    public override string ToString()
    {
        return Text;
    }
}