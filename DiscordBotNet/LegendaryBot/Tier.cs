namespace DiscordBotNet.LegendaryBot;

public enum Tier : byte
{
    Unranked, Bronze, Silver, Gold, Platinum, Diamond, Divine
}

public static class TierExtensions
{

    public static Rarity ToRarity(this Tier tier)
    {
        return (Rarity) (int)tier;
    }
}