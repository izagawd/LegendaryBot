using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Moves;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class GoingAllOut : Blessing
{
    
    public override string Name => "Going All Out";

    [BattleEventListenerMethod]
    public  void IncreaseUlt(CharacterPreDamageEventArgs eventArgs)
    {
        var usedMove = (eventArgs.DamageArgs.DamageSource as MoveDamageSource)?.Move as Ultimate;
        if(usedMove is null) return;
        if (usedMove.User == Character)
        {
            eventArgs.DamageArgs.Damage *= (100 + GetUltimateDamageBoostPercent(LevelMilestone)) * 0.01f;
        }

        
    }
    public override string GetDescription(int levelMilestone)
    {
        return $"Damage dealt by ultimate is increased by {GetUltimateDamageBoostPercent(levelMilestone)}%";
    }

    public int GetUltimateDamageBoostPercent(int levelMilestone)
    {
        if (levelMilestone >= 6) return 20;
        if (levelMilestone >= 5) return 18;
        if (levelMilestone >= 4) return 16;
        if (levelMilestone >= 3) return 14;
        if (levelMilestone >= 2) return 12;
        if (levelMilestone >= 1) return 10;
        return 8;
        
    }
    public override Rarity Rarity => Rarity.FourStar;
    public override int TypeId
    {
        get => 3;
        protected init {}
    }



}