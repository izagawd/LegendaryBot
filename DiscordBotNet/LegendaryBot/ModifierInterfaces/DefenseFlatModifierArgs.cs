using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class DefenseFlatModifierArgs: StatsModifierArgs
{
    public DefenseFlatModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect, valueToChangeWith)
    {
    }
}