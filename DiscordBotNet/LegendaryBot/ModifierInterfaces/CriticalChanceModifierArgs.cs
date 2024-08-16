using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class CriticalChanceModifierArgs : StatsModifierArgs
{
    public CriticalChanceModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect,
        valueToChangeWith)
    {
    }
}