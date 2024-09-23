using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class MaxHealthPercentageModifierArgs : StatsModifierArgs
{
    public MaxHealthPercentageModifierArgs(Character characterToAffect, float valueToChangeWith) :
        base(
            characterToAffect, valueToChangeWith)
    {
    }
}