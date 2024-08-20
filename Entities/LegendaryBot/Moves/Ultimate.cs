using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Moves;

public abstract class Ultimate : Special
{
    public Ultimate(CharacterPartials_Character user) : base(user)
    {
    }
}