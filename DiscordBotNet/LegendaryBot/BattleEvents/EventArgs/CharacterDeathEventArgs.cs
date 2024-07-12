using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterDeathEventArgs : BattleEventArgs
{
    /// <summary>
    /// The character that died
    /// </summary>
    public Character Killed { get;}

    public CharacterDeathEventArgs(Character killed)
    {
        Killed = killed;
    }
}