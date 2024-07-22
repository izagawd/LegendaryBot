using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
    [NotMapped]
    protected int LevelMilestone
    {
        get
        {
            if (Level >= 60)
                return 6;
            if (Level >= 50)
                return 5;
            if (Level >= 40)
                return 4;
            if (Level >= 30)
                return 3;
            if (Level >= 20)
                return 2;
            if (Level >= 10)
                return 1;
            return 0;
        }
    }

    [NotMapped]
    public float BaseAttack => GetBaseAttack(Level, LevelMilestone);

    [NotMapped]
    public float BaseEffectiveness => GetBaseEffectiveness(Level, LevelMilestone);

    [NotMapped]
    public float BaseSpeed => GetBaseSpeed(Level, LevelMilestone);

    [NotMapped]
    public float BaseResistance => GetBaseResistance(Level, LevelMilestone);

    [NotMapped]
    public float BaseCriticalChance => GetBaseCriticalChance(Level, LevelMilestone);

    [NotMapped]
    public float BaseCriticalDamage => GetBaseCriticalDamage(Level, LevelMilestone);

    [NotMapped]
    public  float BaseDefense => GetBaseDefense(Level,LevelMilestone);

    [NotMapped]
    public  float BaseMaxHealth => GetBaseMaxHealth(Level,LevelMilestone);

    
    [NotMapped]
    protected virtual float BaseAttackMultiplier => 1.0f;



    [NotMapped]
    protected virtual float BaseSpeedMultiplier =>  1.0f;



    [NotMapped]
    protected virtual float BaseDefenseMultiplier => 1.0f;

    [NotMapped]
    protected virtual float BaseMaxHealthMultiplier => 1.0f;

    public const int MilestoneFlatAttackIncrease = 30;
    public const int MilestoneFlatHealthIncrease = 80;
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
    
    public float GetBaseAttack(int level, int levelMilestone)
    {
        float toCompute = 89 + ((level - 1) * 12);
  

        var count = GetStatsToIncreaseBasedOnLevelMilestone(levelMilestone).Count(i => i == StatType.Attack);
        var percentageToUse = count * GetStatIncreaseMilestoneValue(StatType.Attack);

        toCompute *= BaseAttackMultiplier;
        toCompute += toCompute * percentageToUse * 0.01f;
        toCompute += MilestoneFlatAttackIncrease * levelMilestone;
        return toCompute;
    }

 
    public float GetBaseEffectiveness(int level, int levelMilestone)
    {
        float toCompute = 0;

        var count = GetStatsToIncreaseBasedOnLevelMilestone(levelMilestone).Count(i => i == StatType.Effectiveness);
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.Effectiveness);
        return toCompute;
    }

    public float GetBaseSpeed(int level, int levelMilestone)
    {
        float toCompute = 100;

        var count = GetStatsToIncreaseBasedOnLevelMilestone(levelMilestone).Count(i => i == StatType.Speed);
        toCompute *= BaseSpeedMultiplier;
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.Speed);
        return toCompute;
    }

    public float GetBaseResistance(int level, int levelMilestone)
    {
        float toCompute = 0;

        var count = GetStatsToIncreaseBasedOnLevelMilestone(levelMilestone).Count(i => i == StatType.Resistance);
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
    public float GetBaseCriticalChance(int level, int levelMilestone)
    {
        float toCompute = 15;

        var count = GetStatsToIncreaseBasedOnLevelMilestone(levelMilestone).Count(i => i == StatType.CriticalChance);
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.CriticalChance);
        return toCompute;
    }

    public float GetBaseCriticalDamage(int level, int levelMilestone)
    {
        float toCompute = 150;

        var count = GetStatsToIncreaseBasedOnLevelMilestone(levelMilestone).Count(i => i == StatType.CriticalDamage);
        toCompute += count * GetStatIncreaseMilestoneValue(StatType.CriticalDamage);
        return toCompute;
    }

    public float GetBaseDefense(int level, int levelMilestone)
    {
        var toCompute = 70 + ((level - 1) * 8.5f);
        var count = GetStatsToIncreaseBasedOnLevelMilestone(levelMilestone).Count(i => i == StatType.Defense);
        var percentageToUse = count * GetStatIncreaseMilestoneValue(StatType.Defense);

        toCompute += toCompute * percentageToUse * 0.01f;
        return toCompute * BaseDefenseMultiplier;
    }

    public float GetBaseMaxHealth(int level, int levelMilestone)
    {
        float toCompute = 295 + ((level - 1) * 75);

        var count = GetStatsToIncreaseBasedOnLevelMilestone(levelMilestone).Count(i => i == StatType.MaxHealth);
        var percentageToUse = count * GetStatIncreaseMilestoneValue(StatType.MaxHealth);
        toCompute *= BaseMaxHealthMultiplier;
        toCompute += toCompute * percentageToUse * 0.01f;
        
        toCompute += MilestoneFlatHealthIncrease * levelMilestone;
        return toCompute;
    }
}