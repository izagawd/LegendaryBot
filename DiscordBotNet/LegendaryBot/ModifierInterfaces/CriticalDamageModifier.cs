using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class CriticalDamageModifierArgs : StatsModifierArgs
{
    public CriticalDamageModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect,
        valueToChangeWith)
    {
    }
}