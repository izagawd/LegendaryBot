using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class TurnEndEventArgs : BattleEventArgs
{
    public TurnEndEventArgs(CharacterPartials_Character character)
    {
        Character = character;
    }

    /// <summary>
    ///     The character that the turn was ended with
    /// </summary>
    public CharacterPartials_Character Character { get; }
}