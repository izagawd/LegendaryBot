using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Rewards;

public class TextReward : Reward
{
    public string Text { get;  }
    public override Reward MergeWith(Reward reward)
    {
        if (reward is not TextReward textReward) throw new Exception("Invalid reward type to merge");
        return new TextReward($"{Text}\n{textReward.Text}\n");
    }


    public TextReward(string text)
    {
        Text = text;
    }
    public override string GiveRewardTo(UserData userData)
    {
        return Text;
    }
}