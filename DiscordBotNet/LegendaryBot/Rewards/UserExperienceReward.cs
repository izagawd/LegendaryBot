using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Rewards;

public class UserExperienceReward : Reward
{
    
    public long Experience { get; }
    public override bool IsValid => Experience > 0;

    public UserExperienceReward(long experience)
    {
        Experience = experience;
    }
    public override Reward MergeWith(Reward reward)
    {
        if (reward is not UserExperienceReward experienceReward) throw new Exception("Provided reward is not of same type");
        return new UserExperienceReward(Experience + experienceReward.Experience);
    }

    public override string GiveRewardTo(UserData userData)
    {
        return userData.IncreaseExp(Experience).Text;
    }
}