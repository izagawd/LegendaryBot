using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;

public abstract class BasicAttack : Move
{
    protected BasicAttack(Character user) : base(user)
    {
    }


    public sealed override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    public sealed override MoveUsageResult Utilize(Character target, MoveUsageType moveUsageType)
    {
        return base.Utilize(target, moveUsageType);
    }
}