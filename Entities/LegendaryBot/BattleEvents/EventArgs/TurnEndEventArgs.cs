using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

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