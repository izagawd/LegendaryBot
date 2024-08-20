using Entities.LegendaryBot.Results;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class CharacterPostDamageEventArgs : BattleEventArgs
{
    public CharacterPostDamageEventArgs(DamageResult damageResult)
    {
        DamageResult = damageResult;
    }

    public DamageResult DamageResult { get; }
}