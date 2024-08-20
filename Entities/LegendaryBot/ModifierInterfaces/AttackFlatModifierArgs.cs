using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class AttackFlatModifierArgs : StatsModifierArgs
{
    public AttackFlatModifierArgs(CharacterPartials_Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect,
        valueToChangeWith)
    {
    }
}