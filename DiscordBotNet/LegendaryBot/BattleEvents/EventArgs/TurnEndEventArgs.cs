using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class TurnEndEventArgs : BattleEventArgs
{
    public TurnEndEventArgs(Character character)
    {
        Character = character;
    }

    /// <summary>
    ///     The character that the turn was ended with
    /// </summary>
    public Character Character { get; }
}