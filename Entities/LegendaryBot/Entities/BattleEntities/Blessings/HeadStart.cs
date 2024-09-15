using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;

namespace Entities.LegendaryBot.Entities.BattleEntities.Blessings;

public class HeadStart : Blessing
{
    private const int CombatReadinessIncreaseAmount = 5;
    public override string Name => "Head Start";

    public override int TypeId
    {
        get => 2;
        protected init { }
    }


    public override Rarity Rarity => Rarity.FiveStar;

    public override string Description =>
        $"Increases combat readiness of the owner at the beginning" +
        $" of the battle by {CombatReadinessIncreaseAmount}%!";


    [BattleEventListenerMethod]
    public void OnStart(BattleBeginEventArgs eventArgs)
    {
        if (Character!.IsDead) return;
        Character.IncreaseCombatReadiness(CombatReadinessIncreaseAmount);
    }
}