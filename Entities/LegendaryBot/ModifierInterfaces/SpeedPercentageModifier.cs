using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class SpeedPercentageModifierArgs : StatsModifierArgs
{
    public SpeedPercentageModifierArgs(CharacterPartials_Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect,
        valueToChangeWith)
    {
    }
}