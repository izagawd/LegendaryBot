using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public abstract class DamageSource
{
    public virtual void OnPostDamage(DamageResult damageResult)
    {
        
    }
    public virtual object Source { get; }

}

public class StatusEffectDamageSource : DamageSource
{
    public override object Source => StatusEffect;
    public StatusEffect StatusEffect { get; }

    public StatusEffectDamageSource(StatusEffect statusEffect)
    {
        StatusEffect = statusEffect;
    }
}
public class MoveDamageSource : DamageSource
{
    public override void OnPostDamage(DamageResult damageResult)
    {
        base.OnPostDamage(damageResult);
        MoveUsageContext.DamageResults.Add(damageResult);
    }

    public Move Move => MoveUsageContext.Move;
    public override object Source => MoveUsageContext.Move;
    public MoveDamageSource(MoveUsageContext moveUsageContext)
    {
        MoveUsageContext = moveUsageContext;
    }

    
    public MoveUsageContext MoveUsageContext { get; }

}