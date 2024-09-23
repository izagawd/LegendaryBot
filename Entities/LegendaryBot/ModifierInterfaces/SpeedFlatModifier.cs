using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class SpeedFlatModifierArgs : StatsModifierArgs
{
    public SpeedFlatModifierArgs(Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect,
        valueToChangeWith)
    {
    }

    public float FlatSpeed { get; }
}