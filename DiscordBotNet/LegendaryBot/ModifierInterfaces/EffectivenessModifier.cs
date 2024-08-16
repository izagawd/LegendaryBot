using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class EffectivenessModifierArgs : StatsModifierArgs
{
    public EffectivenessModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect,
        valueToChangeWith)
    {
    }
}