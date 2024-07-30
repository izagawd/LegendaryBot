using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class HeadStart : Blessing
{
    public override string Name => "Head Start";

    public HeadStart()
    {
        TypeId = 2;
    }

    public override Rarity Rarity => Rarity.FiveStar;

    public float GetCombatReadinessIncreaseAmount(int levelMilestone)
    {
        if (levelMilestone >= 6) return 5;
        if (levelMilestone >= 5) return 4.5f;
        if (levelMilestone >= 4) return 4;
        if (levelMilestone >= 3) return 3.5f;
        if (levelMilestone >= 2) return 3;
        if (levelMilestone >= 1) return 2.5f;
        return 2f;
    }
    public override string GetDescription(int levelMilestone)=> 
            $"Increases combat readiness of the owner at the beginning of the battle by {GetCombatReadinessIncreaseAmount(levelMilestone)}%!";
    
    [BattleEventListenerMethod]
    public  void OnStart(BattleBeginEventArgs eventArgs)
    {
        if(Character!.IsDead) return;
        Character.IncreaseCombatReadiness(GetCombatReadinessIncreaseAmount(LevelMilestone));
    }
}