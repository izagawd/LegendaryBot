using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class SpeedPercentageModifierArgs : StatsModifierArgs
{
    public SpeedPercentageModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect,
        valueToChangeWith)
    {
    }
}