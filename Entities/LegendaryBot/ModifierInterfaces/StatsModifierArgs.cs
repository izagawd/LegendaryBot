using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.ModifierInterfaces;

public abstract class StatsModifierArgs
{
    public StatsModifierArgs(CharacterPartials_Character characterToAffect, float valueToChangeWith)
    {
        CharacterToAffect = characterToAffect;
        ValueToChangeWith = valueToChangeWith;
    }

    /// <summary>
    ///     The value to either increase or decrease with
    /// </summary>
    public float ValueToChangeWith { get; }

    public CharacterPartials_Character CharacterToAffect { get; }
}