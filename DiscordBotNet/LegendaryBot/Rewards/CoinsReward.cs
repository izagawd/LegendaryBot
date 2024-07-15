using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Rewards;
/// <summary>
/// Rewards the user with coins
/// </summary>
public class CoinsReward : Reward
{
    public long Coins { get; }
    public override Reward MergeWith(Reward reward)
    {
        if (reward is not CoinsReward coinsReward) throw new Exception("Reward type given is not of same type");
        return new CoinsReward(Coins + coinsReward.Coins);
    }

    public override bool IsValid => Coins > 0;
  
    public CoinsReward(long coins)
    {
        Coins = coins;
    }

    public override string GiveRewardTo(UserData userData)
    {
        userData.Coins += Coins;
        return $"{userData.Name} Gained {Coins} coins!";
    }
}