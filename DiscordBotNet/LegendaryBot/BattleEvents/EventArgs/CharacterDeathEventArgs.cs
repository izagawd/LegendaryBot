using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

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