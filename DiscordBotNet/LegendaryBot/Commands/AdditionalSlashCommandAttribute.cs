namespace DiscordBotNet.LegendaryBot.command;

public class AdditionalCommandAttribute : Attribute
{
    public string? Example { get; }
    public BotCommandType BotCommandType { get; }
    public AdditionalCommandAttribute(string example, BotCommandType botCommandType)
    {
        Example = example;
        BotCommandType = botCommandType;
    }

    public AdditionalCommandAttribute(string example)
    {
        Example = example;
        BotCommandType = BotCommandType.Other;
    }
    public AdditionalCommandAttribute(BotCommandType botCommandType, string example)
        : this(example, botCommandType){}
    public AdditionalCommandAttribute(BotCommandType botCommandType)
    {
  
        BotCommandType = botCommandType;
    }
}