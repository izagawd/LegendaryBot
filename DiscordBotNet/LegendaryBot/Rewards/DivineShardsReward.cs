using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Rewards;

public class DivineShardsReward : Reward
{
    public int DivineShards { get; }
    public override Reward MergeWith(Reward reward)
    {
        if (reward is DivineShardsReward divineShardsReward)
        {
            return new DivineShardsReward(DivineShards + divineShardsReward.DivineShards);
        }
        throw new Exception();
    }

    public override bool IsValid => DivineShards > 0;

    public DivineShardsReward(int divineShards)
    {
        if (divineShards < 0)
            throw new Exception("Divine shard must be at least 0");
        DivineShards = divineShards;
    }
    public override string GiveRewardTo(UserData userData)
    {
        userData.DivineShards += DivineShards;
        return $"{userData.Name} acquired {DivineShards} Divine Shards!";
    }
}