using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Results;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class CharacterPreDamageEventArgs : BattleEventArgs
{
    public Character DamageReceiver { get;  }
    public CharacterPreDamageEventArgs(DamageArgs damageArgs, Character damageReceiver)
    {
        DamageArgs = damageArgs;
        DamageReceiver = damageReceiver;
    }

    public DamageArgs DamageArgs { get; }
}