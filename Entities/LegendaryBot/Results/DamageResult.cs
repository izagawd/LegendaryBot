using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Results;

public class DamageResult
{
    public DamageSource DamageSource { get; init; }
    public bool IsFixedDamage { get; init; } = false;

    public float DamageDealt { get; init; }
    public bool WasCrit { get; init; }
    public bool CanBeCountered { get; init; } = true;
    public required CharacterPartials_Character DamageReceiver { get; init; }
}