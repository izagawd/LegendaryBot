using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class BlessingOfThePhoenix : Blessing, IBattleEventListener
{
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;

    public int GetHealthPercentRecovering(int level)
    {
        if (level >= 15) return 10;
        if (level >= 12) return 9;
        if (level >= 9) return 8;
        if (level >= 6) return 7;
        if (level >= 3) return 6;
        return 2;
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