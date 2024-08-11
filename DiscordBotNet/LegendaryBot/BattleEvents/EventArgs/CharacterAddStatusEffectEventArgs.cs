using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterStatusEffectAppliedEventArgs : BattleEventArgs
{

    public StatusEffect AddedStatusEffect { get;}

    public CharacterStatusEffectAppliedEventArgs(StatusEffect addedStatusEffect)
    {
        AddedStatusEffect = addedStatusEffect;
    }

}