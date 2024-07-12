using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class TurnStartEventArgs : BattleEventArgs
{
    /// <summary>
    /// The character that the turn was started with
    /// </summary>
    public Character Character { get;  }

    public TurnStartEventArgs(Character character)
    {
        Character = character;
    }
}