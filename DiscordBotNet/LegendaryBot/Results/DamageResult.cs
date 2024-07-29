using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Results;

public struct MoveUsageDetails
{
    public Move Move { get; }
    public MoveUsageType MoveUsageType { get; }

    public MoveUsageDetails(Move move, MoveUsageType moveUsageType)
    {
        Move = move;
        MoveUsageType = moveUsageType;
    }
}
public class DamageResult
{

    public  DamageSource DamageSource { get; init; }
    public bool IsFixedDamage { get; init; } = false;

    public float DamageDealt { get; init; }
    public bool WasCrit { get; init; }
    public bool CanBeCountered { get; init; } = true;
    public required Character DamageReceiver { get; init; }

}