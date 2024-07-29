using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
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
        UsageContext.DamageResults.Add(damageResult);
    }

    public Move Move => UsageContext.Move;
    public override object Source => UsageContext.Move;
    public MoveDamageSource(UsageContext usageContext)
    {
        UsageContext = usageContext;
    }

    
    public UsageContext UsageContext { get; }

}