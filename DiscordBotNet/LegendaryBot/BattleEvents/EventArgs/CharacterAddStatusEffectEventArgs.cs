using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterStatusEffectAppliedEventArgs : BattleEventArgs
{

    public required StatusEffect AddedStatusEffect { get; init; }

}