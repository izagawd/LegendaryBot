using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class SpeedPercentageModifierArgs : StatsModifierArgs
{
    public SpeedPercentageModifierArgs(Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect,
        valueToChangeWith)
    {
    }
}