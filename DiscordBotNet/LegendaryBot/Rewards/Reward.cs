using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Rewards;

public abstract class Reward : IComparable<Reward>
{
    /// <summary>
    ///     The lower the number, the more important or the higher in value it is
    /// </summary>
    [NotMapped]
    public virtual int Priority => 1;

    private static IEnumerable<Type> RewardTypes { get; } =
        typeof(Reward).Assembly.GetTypes().Where(i => i.IsSubclassOf(typeof(Reward)) && !i.IsAbstract);

    public virtual bool IsValid => true;

    public int CompareTo(Reward? other)
    {
        if (other is null) return 1;
        if (other.Priority == Priority) return 0;
        if (Priority > other.Priority) return 1;
        return -1;
    }

    /// <summary>
    ///     Merges all rewards in a collection to avoid rewards of the same type from appearing more than once.
    ///     eg reward called coinsreward merges with another coinsreward. the merged version will have a coins property value
    ///     of the sum of both coinsrewards
    /// </summary>
    /// <param name="rewardsToMerge">the merged version that doesnt have duplicate reward types</param>
    /// <returns></returns>
    public static IEnumerable<Reward> MergeAllRewards(IEnumerable<Reward> rewardsToMerge)
    {
        var asArray = rewardsToMerge.Where(i => i is not null).ToList();
        Dictionary<Type, Reward> mergedRewards = [];

        foreach (var i in RewardTypes)
        foreach (var j in asArray.Where(k => k.GetType() == i))
            if (!mergedRewards.ContainsKey(i))
                mergedRewards[i] = j;
            else
                mergedRewards[i] = j.MergeWith(mergedRewards[i]);

        return mergedRewards.Select(i => i.Value);
    }

    /// <summary>
    ///     Merges a reward with another if possible and returns it's merged form
    ///     eg coin reward merging will return a coin reward with the coin values added
    ///     together
    /// </summary>
    /// <returns></returns>
    public abstract Reward MergeWith(Reward reward);

    public abstract string GiveRewardTo(UserData userData);
}