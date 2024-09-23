using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Results;

namespace Entities.LegendaryBot.Moves;

public abstract class BasicAttack : Move
{
    protected BasicAttack(Character user) : base(user)
    {
    }


    public sealed override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.BattleTeam != User.BattleTeam && !i.IsDead);
    }

    public sealed override MoveUsageResult Utilize(Character target, MoveUsageType moveUsageType)
    {
        return base.Utilize(target, moveUsageType);
    }
}