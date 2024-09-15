using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Moves;

namespace Entities.LegendaryBot.Entities.BattleEntities.Blessings;

public class GoingAllOut : Blessing
{
    private const int DamageIncreasePercent = 20;
    public override string Name => "Going All Out";
    public override Rarity Rarity => Rarity.FourStar;

    public override int TypeId
    {
        get => 3;
        protected init { }
    }

    public override string Description => $"Damage dealt by ultimate is increased by {DamageIncreasePercent}%";

    [BattleEventListenerMethod]
    public void IncreaseUlt(CharacterPreDamageEventArgs eventArgs)
    {
        var usedMove = (eventArgs.DamageArgs.DamageSource as MoveDamageSource)?.Move as Ultimate;
        if (usedMove is null) return;
        if (usedMove.User == Character)
            eventArgs.DamageArgs.Damage *= (100 + DamageIncreasePercent) * 0.01f;
    }
}