using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class MaxHealthFlatModifierArgs: StatsModifierArgs
{
    public MaxHealthFlatModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect, valueToChangeWith)
    {
    }
}