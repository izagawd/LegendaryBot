using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Results;

public class DamageArgs
{
    public bool IsFixedDamage { get; set; } = false;
    /// <summary>
    /// if null, will not consider element for damage calculation
    /// </summary>
    public required Element? ElementToDamageWith { get; set; }
    /// <summary>
    /// The percentage of defense to ignore if possible
    /// </summary>
    public int DefenseToIgnore { get; set; } = 0;

    public Move? Move { get; private set; }
    public StatusEffect? StatusEffect { get; private set; }

    public DamageArgs(Move move)
    {
        Move = move;
    }

    public bool IsCausedByMove => Move is not null;
    public bool IsCausedByStatusEffect => StatusEffect is not null;
    public DamageArgs(StatusEffect statusEffect)
    {
        StatusEffect = statusEffect;
    }
    public required float Damage
    {
        get; set;
    }
    /// <summary>
    /// The one who casted the attack
    /// </summary>
        public required Character DamageDealer { get; init; }
        /// <summary>
        /// Use $ in the string and it will be replaced with the damage
        /// </summary>
        public string? DamageText { get; init; } = null;

        public bool CanBeCountered
        {
            get;
            init;
        } = true;


        public  float CriticalDamage { get; set; } = 150;
        public  float CriticalChance { get; set; } = 50;
        /// <summary>
        /// if true, the attack always lands a critical hit. Doesnt work in fixed damage
        /// </summary>
        public bool AlwaysCrits { get; init; } = false;
        /// <summary>
        /// attack can always crit. doesnt work in fixed damage
        /// </summary>
        
        public bool CanCrit { get; init; } = true;

}