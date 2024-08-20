namespace DiscordBot.Commands;

[AttributeUsage(AttributeTargets.Method)]
public class BotCommandCategoryAttribute : Attribute
{
    public BotCommandCategoryAttribute(BotCommandCategory botCommandCategory = BotCommandCategory.Other)
    {
        BotCommandCategory = botCommandCategory;
    }

    public BotCommandCategory BotCommandCategory { get; }
}