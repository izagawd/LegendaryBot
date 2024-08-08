using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class DefensePercentageModifierArgs : StatsModifierArgs
{
    public DefensePercentageModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect, valueToChangeWith)
    {
    }
}