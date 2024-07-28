namespace DiscordBotNet.LegendaryBot.Commands;

public class AdditionalCommandAttribute : Attribute
{
    public string? Example { get; }
    public BotCommandCategory BotCommandCategory { get; }
    public AdditionalCommandAttribute(string example, BotCommandCategory botCommandCategory = BotCommandCategory.Other)
    {
        Example = example;
        BotCommandCategory = botCommandCategory;
    }


}