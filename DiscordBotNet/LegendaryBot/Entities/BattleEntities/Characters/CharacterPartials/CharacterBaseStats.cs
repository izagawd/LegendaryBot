using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
    [NotMapped]
    public float BaseAttack => GetBaseAttack(Level, Ascension);

    [NotMapped]
    public float BaseEffectiveness => GetBaseEffectiveness(Level, Ascension);

    [NotMapped]
    public float BaseSpeed => GetBaseSpeed(Level, Ascension);

    [NotMapped]
    public float BaseResistance => GetBaseResistance(Level, Ascension);

    [NotMapped]
    public float BaseCriticalChance => GetBaseCriticalChance(Level, Ascension);

    [NotMapped]
    public float BaseCriticalDamage => GetBaseCriticalDamage(Level, Ascension);

    [NotMapped]
    public virtual float BaseDefense => GetBaseDefense(Level,Ascension);

    [NotMapped]
    public virtual float BaseMaxHealth => GetBaseMaxHealth(Level,Ascension);

    
    [NotMapped]
    protected virtual float BaseAttackMultiplier => 1.0f;



    [NotMapped]
    protected virtual float BaseSpeedMultiplier =>  1.0f;



    [NotMapped]
    protected virtual float BaseDefenseMultiplier => 1.0f;

    [NotMapped]
    protected virtual float BaseMaxHealthMultiplier => 1.0f;
    public float GetBaseAttack(int level, int ascension)
    {
        float toCompute = 89 + ((level - 1) * 12);
        toCompute +=  30 * (ascension - 1);

        var count = GetStatsToIncreaseBasedOnAscension(ascension).Count(i => i == StatType.Attack);
        var percentageToUse = count * 4;

        toCompute += toCompute * percentageToUse * 0.01f;
        return toCompute * BaseAttackMultiplier;
    }

    // Implement similar methods for other stats
    public float GetBaseEffectiveness(int level, int ascension)
    {
        float toCompute = 100;

        var count = GetStatsToIncreaseBasedOnAscension(ascension).Count(i => i == StatType.Effectiveness);
        toCompute += count * 10;
        return toCompute;
    }

    public float GetBaseSpeed(int level, int ascension)
    {
        float toCompute = 100;

        var count = GetStatsToIncreaseBasedOnAscension(ascension).Count(i => i == StatType.Speed);
        toCompute *= BaseSpeedMultiplier;
        toCompute += count * 6;
        return toCompute;
    }

    public float GetBaseResistance(int level, int ascension)
    {
        float toCompute = 100;

        var count = GetStatsToIncreaseBasedOnAscension(ascension).Count(i => i == StatType.Resistance);
        toCompute += count * 10;
        return toCompute;
    }

    public float GetBaseCriticalChance(int level, int ascension)
    {
        float toCompute = 100;

        var count = GetStatsToIncreaseBasedOnAscension(ascension).Count(i => i == StatType.CriticalChance);
        toCompute += count * 6;
        return toCompute;
    }

    public float GetBaseCriticalDamage(int level, int ascension)
    {
        float toCompute = 100;

        var count = GetStatsToIncreaseBasedOnAscension(ascension).Count(i => i == StatType.CriticalDamage);
        toCompute += count * 10;
        return toCompute;
    }

    public float GetBaseDefense(int level, int ascension)
    {
        float toCompute = 60 + ((level - 1) * 7.5f);
        var count = GetStatsToIncreaseBasedOnAscension(ascension).Count(i => i == StatType.Defense);
        var percentageToUse = count * 4;

        toCompute += toCompute * percentageToUse * 0.01f;
        return toCompute * BaseDefenseMultiplier;
    }

    public float GetBaseMaxHealth(int level, int ascension)
    {
        float toCompute = 295 + ((level - 1) * 75);
        toCompute +=  80 * (ascension - 1);
        var count = GetStatsToIncreaseBasedOnAscension(ascension).Count(i => i == StatType.MaxHealth);
        var percentageToUse = count * 4;

        toCompute += toCompute * percentageToUse * 0.01f;
        return toCompute * BaseMaxHealthMultiplier;
    }
}