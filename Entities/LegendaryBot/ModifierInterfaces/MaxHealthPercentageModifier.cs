using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class MaxHealthPercentageModifierArgs : StatsModifierArgs
{
    public MaxHealthPercentageModifierArgs(CharacterPartials_Character characterToAffect, float valueToChangeWith) :
        base(
            characterToAffect, valueToChangeWith)
    {
    }
}