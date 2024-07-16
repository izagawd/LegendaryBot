using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class HeadStart : Blessing, IBattleEventListener
{
    public override Rarity Rarity => Rarity.FiveStar;

    public float GetCombatReadinessIncreaseAmount(int level)
    {
        if (level >= 60) return 5;
        if (level >= 50) return 4.5f;
        if (level >= 40) return 4;
        if (level >= 30) return 3.5f;
        if (level >= 20) return 3;
        if (level >= 10) return 2.5f;
        return 2f;
    }
    public override string GetDescription(int level)=> 
            $"Increases combat readiness of the owner at the beginning of the battle by {GetCombatReadinessIncreaseAmount(level)}%!";
    
    [BattleEventListenerMethod]
    public  void OnStart(BattleBeginEventArgs eventArgs)
    {
        if(Character!.IsDead) return;
        Character.IncreaseCombatReadiness(GetCombatReadinessIncreaseAmount(Level));
    }
}