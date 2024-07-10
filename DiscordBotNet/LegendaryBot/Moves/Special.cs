using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

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
            return base.ToString() + $" ({Cooldown})";
        return base.ToString();
    }

    public sealed override UsageResult Utilize(Character target, UsageType usageType)
    {
        if (usageType == UsageType.NormalUsage)
        {
            Cooldown = MaxCooldown;
        }
        
        return base.Utilize(target, usageType);
    }
}