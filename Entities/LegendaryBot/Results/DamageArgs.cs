using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.StatusEffects;

namespace Entities.LegendaryBot.Results;




public class DamageArgs
{
    public DamageArgs(float damage, DamageSource damageSource)
    {
        DamageSource = damageSource;
        Damage = damage;
    }

    public DamageSource DamageSource { get; }
    public bool IsFixedDamage { get; init; } = false;

    /// <summary>
    ///     if null, will not consider element for damage calculation
    /// </summary>
    public Element? ElementToDamageWith { get; set; }

    /// <summary>
    ///     The percentage of defense to ignore if possible
    /// </summary>
    public int DefenseToIgnore { get; set; } = 0;


    public StatusEffect? StatusEffect { get; }

    public float Damage { get; set; }

    /// <summary>
    ///     Use $ in the string and it will be replaced with the damage
    /// </summary>
    public string? DamageText { get; init; } = null;

    public bool CanBeCountered { get; init; } = true;


    public float CriticalDamage { get; set; } = 150;
    public float CriticalChance { get; set; } = 50;

    /// <summary>
    ///     if true, the attack always lands a critical hit. Doesnt work in fixed damage
    /// </summary>
    public bool AlwaysCrits { get; set; } = false;

    /// <summary>
    ///     attack can always crit. doesnt work in fixed damage
    /// </summary>

    public bool CanCrit { get; init; } = true;
}