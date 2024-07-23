using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class VitalForce : Blessing, IBattleEventListener, IStatsModifier
{

    public override string GetDescription(int level)
    {
        return $"Defense is increased by {GetDefenseBoost(level)}%";
    }

    public float GetDefenseBoost(int level)
    {
        if (level >= 60) return 10;
        if (level >= 50) return 9;
        if (level >= 40) return 8;
        if (level >= 30) return 7;
        if (level >= 20) return 6;
        if (level >= 10) return 5;
        return 4;
        
    }
    
    
    public override Rarity Rarity => Rarity.ThreeStar;

    public VitalForce()
    {
        TypeId = 4;
    }
    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new DefensePercentageModifierArgs
        {
            CharacterToAffect = Character!,
            ValueToChangeWith = GetDefenseBoost(Level),
        };
    }
}