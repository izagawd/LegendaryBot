using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

public class CharacterReviveEventArgs : BattleEventArgs
{
    public CharacterReviveEventArgs(Character revivedCharacter)
    {
        RevivedCharacter = revivedCharacter;
    }

    public Character RevivedCharacter { get; }
}