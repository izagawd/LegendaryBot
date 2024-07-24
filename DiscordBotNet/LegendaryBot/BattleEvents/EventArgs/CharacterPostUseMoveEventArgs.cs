using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

//best arg to listen to for counter attacks or extra attacks
public class CharacterPostUseMoveEventArgs : BattleEventArgs
{
    public UsageResult UsageResult { get; }

    public CharacterPostUseMoveEventArgs(UsageResult usageResult)
    {
        UsageResult = usageResult;
    }
}