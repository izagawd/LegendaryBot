using Entities.LegendaryBot.Results;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class CharacterPreDamageEventArgs : BattleEventArgs
{
    public CharacterPreDamageEventArgs(DamageArgs damageArgs)
    {
        DamageArgs = damageArgs;
    }

    public DamageArgs DamageArgs { get; private set; }
}