using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.ModifierInterfaces;

public class ResistanceModifierArgs : StatsModifierArgs
{
    public ResistanceModifierArgs(Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect,
        valueToChangeWith)
    {
    }

    private float ResistancePercentage { get; }
}