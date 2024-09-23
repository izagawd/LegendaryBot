using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class TurnStartEventArgs : BattleEventArgs
{
    public TurnStartEventArgs(Character character)
    {
        Character = character;
    }

    /// <summary>
    ///     The character that the turn was started with
    /// </summary>
    public Character Character { get; }
}