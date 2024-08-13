using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class VitalForce : Blessing, IStatsModifier
{
    public override string Name => "Vital Force";

    public override string GetDescription(int levelMilestone)
    {
        return $"Defense is increased by {GetDefenseBoost(levelMilestone)}%";
    }

    public float GetDefenseBoost(int levelMilestone)
    {
        if (levelMilestone >= 6) return 10;
        if (levelMilestone >= 5) return 9;
        if (levelMilestone >= 4) return 8;
        if (levelMilestone >= 3) return 7;
        if (levelMilestone >= 2) return 6;
        if (levelMilestone >= 1) return 5;
        return 4;
        
    }
    
    
    public override Rarity Rarity => Rarity.ThreeStar;


    public override int TypeId
    {
        get => 4;
        protected init {}
    }

    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new DefensePercentageModifierArgs(Character!, GetDefenseBoost(LevelMilestone));
    }
}