using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterStatusEffectAppliedEventArgs : BattleEventArgs
{

    public required StatusEffect AddedStatusEffect { get; init; }

}