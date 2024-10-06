using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;

namespace Entities.LegendaryBot.Entities.BattleEntities.Blessings;

public class ThresholdStrike : Blessing
{
    private const int PercentageDmgIncrease = 10;
    public override string Description => $"Increases damage dealt by a move to an enemy by {PercentageDmgIncrease}% if their health" +
                                          $"is 50% or higher";

    public override Rarity Rarity => Rarity.ThreeStar;
    public override int TypeId
    {
        get => 5; protected init{} }

    
    public void IncreaseDamage(CharacterPreDamageEventArgs damageEventArgs)
    {
        if (damageEventArgs.DamageArgs.DamageSource is MoveDamageSource moveDamageSource

            && moveDamageSource.Character == Character
            && damageEventArgs.DamageReceiver.Health / damageEventArgs.DamageReceiver.MaxHealth >= 0.5f)
        {
            Console.WriteLine($"Strike initiated for {moveDamageSource.Character.NameWithAlphabet}\nPrevious dmg: {damageEventArgs.DamageArgs.Damage}");
            
            damageEventArgs.DamageArgs.Damage += (PercentageDmgIncrease / 100.0f) * damageEventArgs.DamageArgs.Damage;
            Console.WriteLine($"Current dmg: {damageEventArgs.DamageArgs.Damage}");
        }
    }
    public override string Name => "Threshold Strike";
}