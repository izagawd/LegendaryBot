using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Moves;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class GoingAllOut : Blessing, IBattleEventListener
{
    [BattleEventListenerMethod]
    public  void IncreaseUlt(CharacterPreDamageEventArgs eventArgs)
    {

        if (eventArgs.DamageArgs.DamageDealer == Character && eventArgs.DamageArgs
                .MoveUsageDetails?.Move is Ultimate)
        {
            eventArgs.DamageArgs.Damage *= (100 + GetUltimateDamageBoostPercent(Level)) * 0.01f;
        }

        
    }
    public override string GetDescription(int level)
    {
        return $"Damage dealt by ultimate is increased by {GetUltimateDamageBoostPercent(level)}%";
    }

    public int GetUltimateDamageBoostPercent(int level)
    {
        if (level >= 60) return 20;
        if (level >= 50) return 18;
        if (level >= 40) return 16;
        if (level >= 30) return 14;
        if (level >= 20) return 12;
        if (level >= 10) return 10;
        return 8;
        
    }
    public override Rarity Rarity => Rarity.FourStar;

    public GoingAllOut()
    {
        TypeId = 3;
    }

}