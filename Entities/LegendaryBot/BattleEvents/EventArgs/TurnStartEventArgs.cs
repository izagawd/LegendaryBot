using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class TurnStartEventArgs : BattleEventArgs
{
    public TurnStartEventArgs(CharacterPartials_Character character)
    {
        Character = character;
    }

    /// <summary>
    ///     The character that the turn was started with
    /// </summary>
    public CharacterPartials_Character Character { get; }
}