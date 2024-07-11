using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot;

public interface ICanBeLeveledUp
{
    public int Level { get; }
    public long Experience { get; }

    public long GetRequiredExperienceToNextLevel(int level);
    public long GetRequiredExperienceToNextLevel();

    public ExperienceGainResult IncreaseExp(long experienceToGain);
}