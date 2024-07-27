using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public abstract class DamageSource
{
    public virtual object Source { get; }
}

public class StatusEffectDamageSource : DamageSource
{
    public override object Source => StatusEffect;
    public required StatusEffect StatusEffect { get; init; }
}
public class MoveDamageSource : DamageSource
{
    public override object Source => Move;
    public required UsageType UsageType { get; set; }
    public required Move Move { get; set; }

}