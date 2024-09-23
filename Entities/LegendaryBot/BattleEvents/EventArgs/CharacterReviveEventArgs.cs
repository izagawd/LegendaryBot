using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class CharacterReviveEventArgs : BattleEventArgs
{
    public CharacterReviveEventArgs(Character revivedCharacter)
    {
        RevivedCharacter = revivedCharacter;
    }

    public Character RevivedCharacter { get; }
}