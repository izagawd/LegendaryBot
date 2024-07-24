using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterPostUseMoveEventArgs : BattleEventArgs
{
    public UsageResult UsageResult { get; }

    public CharacterPostUseMoveEventArgs(UsageResult usageResult)
    {
        UsageResult = usageResult;
    }
}