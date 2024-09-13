using BasicFunctionality;
using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;

namespace Entities.LegendaryBot.Entities.BattleEntities.Blessings;

public class PowerOfThePhoenix : Blessing
{
    public override string Name => "Power Of The Phoenix";

    public override int TypeId
    {
        get => 1;
        protected init { }
    }


    public override Rarity Rarity => Rarity.FiveStar;

    private const int HealthPercentageRecovering = 5;


    [BattleEventListenerMethod]
    public void HealOnTurnBegin(TurnStartEventArgs eventArgs)
    {
        if (eventArgs.Character != Character) return;

        Character!.RecoverHealth((5 * 0.01 * Character.MaxHealth).Round(),
            $"{Character.NameWithAlphabet} recovered $ health via the blessing of the phoenix");
    }

    public override string Description =>

            $"At the start of the character's turn, they recover {HealthPercentageRecovering}% of their health";
    
}