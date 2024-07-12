using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class TurnEndEventArgs : BattleEventArgs
{
    /// <summary>
    /// The character that the turn was ended with
    /// </summary>
    public Character Character { get;  }

    public TurnEndEventArgs(Character character)
    {
        Character = character;
    }
}