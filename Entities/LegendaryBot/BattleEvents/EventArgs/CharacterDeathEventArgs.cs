using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class CharacterDeathEventArgs : BattleEventArgs
{
    public CharacterDeathEventArgs(CharacterPartials_Character killed)
    {
        Killed = killed;
    }

    /// <summary>
    ///     The character that died
    /// </summary>
    public CharacterPartials_Character Killed { get; }
}