namespace DiscordBotNet.LegendaryBot;

public enum StatType : byte
{
    Attack,
    Defense,
    Speed,
    CriticalChance,
    CriticalDamage,
    MaxHealth,
    Resistance,
    Effectiveness
}

public static class Idk
{
    public static string GetShortName(this StatType statType)
    {
        switch (statType)
        {
            case StatType.Attack:
                return "Atk";
            case StatType.Defense:
                return "Def";
            case StatType.Speed:
                return "Speed";
            case StatType.CriticalChance:
                return "Crit Chance";
            case StatType.CriticalDamage:
                return "Crit Dmg";
            case StatType.MaxHealth:
                return "Max HP";
            case StatType.Resistance:
                return "Res";
            case StatType.Effectiveness:
                return "Eff";
            default:
                throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
        }
    }
}