namespace DiscordBotNet.LegendaryBot.Commands;

public static class BotCommandCategoryExtensions
{
    public static string GetName(this BotCommandCategory category)
    {
        return category.ToString();
    }
}

public enum BotCommandCategory : byte
{
    Other,
    Inventory,
    Character,
    Team,
    Battle
}