using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot;

public interface ICanBeLeveledUp
{
    public int Level { get; }
    public int Experience { get; }

    public int GetRequiredExperienceToNextLevel(int level);

    public int GetRequiredExperienceToNextLevel()
    {
       return GetRequiredExperienceToNextLevel(Level);
    }

    public ExperienceGainResult IncreaseExp(int experienceToGain);
}