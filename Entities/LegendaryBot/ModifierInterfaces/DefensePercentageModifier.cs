using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class DefensePercentageModifierArgs : StatsModifierArgs
{
    public DefensePercentageModifierArgs(Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect,
        valueToChangeWith)
    {
    }
}