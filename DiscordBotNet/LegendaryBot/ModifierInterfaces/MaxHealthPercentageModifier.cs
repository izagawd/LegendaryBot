using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class MaxHealthPercentageModifierArgs : StatsModifierArgs
{
    public MaxHealthPercentageModifierArgs(Character characterToAffect, float valueToChangeWith) : base(
        characterToAffect, valueToChangeWith)
    {
    }
}