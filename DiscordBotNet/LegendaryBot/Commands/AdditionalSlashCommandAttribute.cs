namespace DiscordBotNet.LegendaryBot.Commands;

public class BotCommandCategoryAttribute : Attribute
{

    public BotCommandCategory BotCommandCategory { get; }
    public BotCommandCategoryAttribute( BotCommandCategory botCommandCategory = BotCommandCategory.Other)
    {
        BotCommandCategory = botCommandCategory;
    }


}