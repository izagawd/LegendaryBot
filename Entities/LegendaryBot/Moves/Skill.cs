using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Moves;

public abstract class Skill : Special
{
    public Skill(CharacterPartials_Character user) : base(user)
    {
    }
}