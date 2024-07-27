using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public abstract class DamageSource
{

}

public class StatusEffectDamageSource : DamageSource
{
    public required StatusEffect StatusEffect { get; init; }
}
public class MoveDamageSource : DamageSource
{
    public required UsageType UsageType { get; set; }
    public required Move Move { get; set; }

}