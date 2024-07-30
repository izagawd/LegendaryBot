using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;

public abstract class Special : Move
{
    /// <summary>
    /// The maximum cooldown of the move based on the move level
    /// </summary>

    public abstract int MaxCooldown { get; } 


    /// <summary>
    /// The cooldown of the move
    /// </summary>
    public int Cooldown { get; set; } = 0;
    public bool IsOnCooldown => Cooldown > 0;


    public sealed override bool CanBeUsed()
    {
        return base.CanBeUsed() && !IsOnCooldown;
    }

    public override string ToString()
    {
        if (IsOnCooldown)
            return base.ToString() + $" [{Cooldown}]";
        return base.ToString();
    }

    public sealed override MoveUsageResult Utilize(Character target, MoveUsageType moveUsageType)
    {
        if (moveUsageType == MoveUsageType.NormalUsage)
        {
            Cooldown = MaxCooldown;
        }
        return base.Utilize(target, moveUsageType);
    }
}