using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class ResistanceModifierArgs: StatsModifierArgs
{
    float ResistancePercentage { get; }

    public ResistanceModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect, valueToChangeWith)
    {
    }
}