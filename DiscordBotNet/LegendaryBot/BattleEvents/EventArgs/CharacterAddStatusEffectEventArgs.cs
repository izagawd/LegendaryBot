using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterAddStatusEffectEvent : BattleEventArgs
{
    public Character CharacterThatCausedTheAdding { get; }
    public Character CharacterToAddTo { get;}
    public bool Succeeded { get;  } 
    public StatusEffect AddedStatusEffect { get; }

    public CharacterAddStatusEffectEvent(Character characterToAddTo, Character characterThatCausedTheAdding,
        StatusEffect addedStatusEffect, bool succeeded = true)
    {
        Succeeded = succeeded;
        AddedStatusEffect = addedStatusEffect;
        CharacterToAddTo = characterToAddTo;
        CharacterThatCausedTheAdding = characterThatCausedTheAdding;
    }
}