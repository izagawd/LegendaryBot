namespace Entities.LegendaryBot;

public enum Tier : byte
{
    Unranked,
    Bronze,
    Silver,
    Gold,
    Platinum,
    Diamond,
    Divine
}

public static class TierExtensions
{
    /// <summary>
    /// Converts tier into rarity
    /// </summary>
    public static Rarity ToRarity(this Tier tier)
    {
        if (tier == Tier.Divine)
            return Rarity.FiveStar;
        return (Rarity)(int)tier;
    }
}