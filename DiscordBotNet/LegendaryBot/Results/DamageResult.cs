using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Results;

public struct MoveUsageDetails
{
    public Move Move { get; }
    public UsageType UsageType { get; }

    public MoveUsageDetails(Move move, UsageType usageType)
    {
        Move = move;
        UsageType = usageType;
    }
}
public class DamageResult
{
    public  MoveUsageDetails? MoveUsageDetails { get; }
    public StatusEffect? StatusEffect { get; private set; }

    public DamageResult(Move move, UsageType moveUsageType)
    {
        MoveUsageDetails = new MoveUsageDetails(move, moveUsageType);
    }

    public bool IsFixedDamage { get; set; } = false;
    public DamageResult(){}
    public DamageResult(StatusEffect statusEffect)
    {
        StatusEffect = statusEffect;
    }
    public int Damage { get; init; }
    public bool WasCrit { get; init; }
    public bool CanBeCountered { get; init; } = true;
    public  Character? DamageDealer { get; init; }
    public required Character DamageReceiver { get; init; }

}