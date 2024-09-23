using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class AttackPercentageModifierArgs : StatsModifierArgs
{
    public AttackPercentageModifierArgs(Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect,
        valueToChangeWith)
    {
    }
}