using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
   



    [NotMapped] public float BaseAttack => GetBaseAttack(Level, DivineEcho);

    [NotMapped] public float BaseEffectiveness => GetBaseEffectiveness(Level, DivineEcho);

    [NotMapped] public float BaseSpeed => GetBaseSpeed(Level, DivineEcho);

    [NotMapped] public float BaseResistance => GetBaseResistance(Level, DivineEcho);

    [NotMapped] public float BaseCriticalChance => GetBaseCriticalChance(Level, DivineEcho);

    [NotMapped] public float BaseCriticalDamage => GetBaseCriticalDamage(Level, DivineEcho);

    [NotMapped] public float BaseDefense => GetBaseDefense(Level, DivineEcho);

    [NotMapped] public float BaseMaxHealth => GetBaseMaxHealth(Level, DivineEcho);


    [NotMapped] protected virtual float BaseAttackMultiplier => 1.0f;


    [NotMapped] protected virtual float BaseSpeedMultiplier => 1.0f;


    [NotMapped] protected virtual float BaseDefenseMultiplier => 1.0f;

    [NotMapped] protected virtual float BaseMaxHealthMultiplier => 1.0f;

    public static int GetStatIncreaseMilestoneValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.Attack:
            case StatType.Defense:
            case StatType.MaxHealth:
                return 4;
            case StatType.Resistance:
            case StatType.Effectiveness:
                return 10;
            case StatType.Speed:
                return 6;
            case StatType.CriticalChance:
                return 6;
            case StatType.CriticalDamage:
                return 10;
            default:
                throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
        }
    }

    public float GetBaseAttack(int level, int divineEcho)
    {
        float toCompute = 89 + (level - 1) * 12;


        var count = GetStatsToIncreaseBasedOnDivineEcho(divineEcho).Count(i => i == StatType.Attack);
        var percentageToUse = count * GetStatIncreaseMilestoneValue(StatType.Attack);

        toCompute *= BaseAttackMultiplier;
        toCompute += toCompute * percentageToUse * 0.01f;

        return toCompute;
    }


    public float GetBaseEffectiveness(int level, int divineEcho)
    {
        float toCompute = 0;

        var count = GetStatsToIncreaseBasedOnDivineEcho(divineEcho).Count(i => i == StatType.Effectiveness);
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.Effectiveness);
        return toCompute;
    }

    public float GetBaseSpeed(int level, int divineEcho)
    {
        float toCompute = 100;

        var count = GetStatsToIncreaseBasedOnDivineEcho(divineEcho).Count(i => i == StatType.Speed);
        toCompute *= BaseSpeedMultiplier;
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.Speed);
        return toCompute;
    }

    public float GetBaseResistance(int level, int divineEcho)
    {
        float toCompute = 0;

        var count = GetStatsToIncreaseBasedOnDivineEcho(divineEcho).Count(i => i == StatType.Resistance);
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.Resistance);
        return toCompute;
    }


    public static string GetStatIncreaseMilestoneValueString(StatType statType)
    {
        var asInt = GetStatIncreaseMilestoneValue(statType);
        var asString = asInt.ToString();
        if (statType != StatType.Speed)
            asString += '%';
        return asString;
    }

    public float GetBaseCriticalChance(int level, int divineEcho)
    {
        float toCompute = 15;

        var count = GetStatsToIncreaseBasedOnDivineEcho(divineEcho).Count(i => i == StatType.CriticalChance);
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.CriticalChance);
        return toCompute;
    }

    public float GetBaseCriticalDamage(int level, int divineEcho)
    {
        float toCompute = 150;

        var count = GetStatsToIncreaseBasedOnDivineEcho(divineEcho).Count(i => i == StatType.CriticalDamage);
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.CriticalDamage);
        return toCompute;
    }

    public float GetBaseDefense(int level, int divineEcho)
    {
        var toCompute = 70 + (level - 1) * 8.5f;
        var count = GetStatsToIncreaseBasedOnDivineEcho(divineEcho).Count(i => i == StatType.Defense);
        var percentageToUse = count * GetStatIncreaseMilestoneValue(StatType.Defense);

        toCompute += toCompute * percentageToUse * 0.01f;
        return toCompute * BaseDefenseMultiplier;
    }

    public float GetBaseMaxHealth(int level, int divineEcho)
    {
        float toCompute = 295 + (level - 1) * 75;

        var count = GetStatsToIncreaseBasedOnDivineEcho(divineEcho).Count(i => i == StatType.MaxHealth);
        var percentageToUse = count * GetStatIncreaseMilestoneValue(StatType.MaxHealth);
        toCompute *= BaseMaxHealthMultiplier;
        toCompute += toCompute * percentageToUse * 0.01f;

        return toCompute;
    }
}