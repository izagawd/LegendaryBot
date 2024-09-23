using Entities.LegendaryBot.Results;
using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.Moves;

public abstract class Special : Move
{
    public Special(Character user) : base(user)
    {
    }

    /// <summary>
    ///     The maximum cooldown of the move based on the move level
    /// </summary>

    public abstract int MaxCooldown { get; }


    /// <summary>
    ///     The cooldown of the move
    /// </summary>
    public int Cooldown { get; set; }

    public bool IsOnCooldown => Cooldown > 0;


    public  override bool CanBeUsedNormally()
    {
        return base.CanBeUsedNormally() && !IsOnCooldown;
    }

    public override string ToString()
    {
        if (IsOnCooldown)
            return base.ToString() + $" [{Cooldown}]";
        return base.ToString();
    }

    public sealed override MoveUsageResult Utilize(Character target, MoveUsageType moveUsageType)
    {
        if (moveUsageType == MoveUsageType.NormalUsage) Cooldown = MaxCooldown;
        return base.Utilize(target, moveUsageType);
    }
}