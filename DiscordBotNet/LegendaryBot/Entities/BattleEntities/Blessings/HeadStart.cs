using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class HeadStart : Blessing, IBattleEventListener
{
    public override Rarity Rarity => Rarity.FiveStar;

    public int GetCombatReadinessIncreaseAmount(int level)
    {
        if (level >= 15) return 10;
        if (level >= 12) return 9;
        if (level >= 9) return 8;
        if (level >= 6) return 7;
        if (level >= 3) return 6;
        return 5;
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