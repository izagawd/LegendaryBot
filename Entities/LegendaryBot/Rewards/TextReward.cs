using Entities.Models;

namespace Entities.LegendaryBot.Rewards;

public class TextReward : Reward
{
    public TextReward(string text)
    {
        Text = text;
    }

    public string Text { get; }

    public override Reward MergeWith(Reward reward)
    {
        if (reward is not TextReward textReward) throw new Exception("Invalid reward type to merge");
        return new TextReward($"{Text}\n{textReward.Text}\n");
    }

    public override Task<string> GiveRewardToAsync(UserData userData, IQueryable<UserData> userDataQueryable)
    {
        return Task.FromResult(Text);
    }
}