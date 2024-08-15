using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using Functionality;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class PowerOfThePhoenix : Blessing
{
    public override string Name => "Power Of The Phoenix";
    public override int TypeId
    {
        get => 1;
        protected init {}
    }


    public override Rarity Rarity => Rarity.FiveStar;

    public int GetHealthPercentRecovering(int levelMilestone)
    {
        if (levelMilestone >= 6) return 10;
        if (levelMilestone >= 5) return 9;
        if (levelMilestone >= 4) return 8;
        if (levelMilestone >= 3) return 7;
        if (levelMilestone >= 2) return 6;
        if (levelMilestone >= 1) return 5;
        return 4;
    }
    [BattleEventListenerMethod]
    public  void HealOnTurnBegin(TurnStartEventArgs eventArgs)
    {
    
        if (eventArgs.Character != Character) return;

         Character!.RecoverHealth((GetHealthPercentRecovering(LevelMilestone) *  0.01 * Character.MaxHealth).Round(),$"{Character.NameWithAlphabet} recovered $ health via the blessing of the phoenix");
  

    }

    public override string GetDescription(int levelMilestone)
    {
        return $"At the start of the character's turn, they recover {GetHealthPercentRecovering(levelMilestone)}% of their health";
    }

  

}