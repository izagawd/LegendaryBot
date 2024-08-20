using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class EffectivenessModifierArgs : StatsModifierArgs
{
    public EffectivenessModifierArgs(CharacterPartials_Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect,
        valueToChangeWith)
    {
    }
}