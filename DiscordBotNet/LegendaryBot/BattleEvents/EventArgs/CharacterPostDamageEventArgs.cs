using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterPostDamageEventArgs : BattleEventArgs
{
    public CharacterPostDamageEventArgs(DamageResult damageResult)
    {
        DamageResult = damageResult;
    }

    public DamageResult DamageResult { get; }
}