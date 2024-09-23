using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class CharacterDeathEventArgs : BattleEventArgs
{
    public CharacterDeathEventArgs(Character killed)
    {
        Killed = killed;
    }

    /// <summary>
    ///     The character that died
    /// </summary>
    public Character Killed { get; }
}