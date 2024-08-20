using Entities.LegendaryBot.Results;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Moves;

public abstract class BasicAttack : Move
{
    protected BasicAttack(CharacterPartials_Character user) : base(user)
    {
    }


    public sealed override IEnumerable<CharacterPartials_Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    public sealed override MoveUsageResult Utilize(CharacterPartials_Character target, MoveUsageType moveUsageType)
    {
        return base.Utilize(target, moveUsageType);
    }
}