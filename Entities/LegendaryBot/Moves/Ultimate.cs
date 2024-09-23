using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.Moves;

public abstract class Ultimate : Special
{
    public Ultimate(Character user) : base(user)
    {
    }
}