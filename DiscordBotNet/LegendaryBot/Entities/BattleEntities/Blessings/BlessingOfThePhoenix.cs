using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class BlessingOfThePhoenix : Blessing, IBattleEventListener
{
    public override Rarity Rarity => Rarity.FiveStar;

    public int GetHealthPercentRecovering(int level)
    {
        if (level >= 60) return 10;
        if (level >= 50) return 9;
        if (level >= 40) return 8;
        if (level >= 30) return 7;
        if (level >= 20) return 6;
        if (level >= 10) return 5;
        return 4;
    }
    [BattleEventListenerMethod]
    public  void HealOnTurnBegin(TurnStartEventArgs eventArgs)
    {
    
        if (eventArgs.Character != Character) return;

         Character!.RecoverHealth((GetHealthPercentRecovering(Level) *  0.01 * Character.MaxHealth).Round(),$"{Character.NameWithAlphabetIdentifier} recovered $ health via the blessing of the phoenix");
  

    }

    public override string GetDescription(int level)
    {
        return $"At the start of the character's turn, they recover {GetHealthPercentRecovering(level)}% of their health";
    }
}