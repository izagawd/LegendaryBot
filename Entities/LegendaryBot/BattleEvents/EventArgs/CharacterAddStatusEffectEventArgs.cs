using Entities.LegendaryBot.StatusEffects;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class CharacterStatusEffectAppliedEventArgs : BattleEventArgs
{
    public CharacterStatusEffectAppliedEventArgs(StatusEffect addedStatusEffect)
    {
        AddedStatusEffect = addedStatusEffect;
    }

    public StatusEffect AddedStatusEffect { get; }
}