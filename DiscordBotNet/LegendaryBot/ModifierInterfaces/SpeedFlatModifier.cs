using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public class SpeedFlatModifierArgs: StatsModifierArgs
{
    public float FlatSpeed { get; }

    public SpeedFlatModifierArgs(Character characterToAffect, float valueToChangeWith) : base(characterToAffect, valueToChangeWith)
    {
    }
}