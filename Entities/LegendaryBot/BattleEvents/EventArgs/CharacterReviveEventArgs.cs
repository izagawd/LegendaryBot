using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.BattleEvents.EventArgs;

public class CharacterReviveEventArgs : BattleEventArgs
{
    public CharacterReviveEventArgs(CharacterPartials_Character revivedCharacter)
    {
        RevivedCharacter = revivedCharacter;
    }

    public CharacterPartials_Character RevivedCharacter { get; }
}