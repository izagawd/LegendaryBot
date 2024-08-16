using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public abstract class StatsModifierArgs
{
    public StatsModifierArgs(Character characterToAffect, float valueToChangeWith)
    {
        CharacterToAffect = characterToAffect;
        ValueToChangeWith = valueToChangeWith;
    }

    /// <summary>
    ///     The value to either increase or decrease with
    /// </summary>
    public float ValueToChangeWith { get; }

    public Character CharacterToAffect { get; }
}