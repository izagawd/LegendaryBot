using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public abstract class DamageSource
{
    public virtual object Source { get; }

    public virtual void OnPostDamage(DamageResult damageResult)
    {
    }
}

public class StatusEffectDamageSource : DamageSource
{
    public StatusEffectDamageSource(StatusEffect statusEffect)
    {
        StatusEffect = statusEffect;
    }

    public override object Source => StatusEffect;
    public StatusEffect StatusEffect { get; }
}

public class MoveDamageSource : DamageSource
{
    public MoveDamageSource(MoveUsageContext moveUsageContext)
    {
        MoveUsageContext = moveUsageContext;
    }

    public Move Move => MoveUsageContext.Move;
    public override object Source => MoveUsageContext.Move;


    public MoveUsageContext MoveUsageContext { get; }

    public override void OnPostDamage(DamageResult damageResult)
    {
        base.OnPostDamage(damageResult);
        MoveUsageContext.DamageResults.Add(damageResult);
    }
}