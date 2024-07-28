namespace DiscordBotNet.LegendaryBot.Commands;

[AttributeUsage(AttributeTargets.Method)]
public class BotCommandCategoryAttribute : Attribute
{

    public BotCommandCategory BotCommandCategory { get; }
    public BotCommandCategoryAttribute( BotCommandCategory botCommandCategory = BotCommandCategory.Other)
    {
        BotCommandCategory = botCommandCategory;
    }


}