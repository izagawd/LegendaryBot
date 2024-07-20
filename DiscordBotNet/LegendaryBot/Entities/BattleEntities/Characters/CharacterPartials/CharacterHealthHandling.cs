using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
    /// <summary>
    /// Recovers the health of this character
    /// </summary>
    /// <param name="toRecover">Amount to recover</param>
    /// <param name="recoveryText">text to say when health recovered. use $ to represent health recovered</param>
    /// <returns>Amount recovered</returns>
    public virtual int RecoverHealth(float toRecover,
        string? recoveryText = null, bool announceHealing = true)
    {
        if (IsDead) return 0;
        var healthToRecover = toRecover.Round();

        Health += healthToRecover;
        if (recoveryText is null)
            recoveryText = $"{NameWithAlphabetIdentifier} recovered $ health!";
        if(announceHealing)
            CurrentBattle.AddAdditionalBattleText(recoveryText.Replace("$",healthToRecover.ToString()));
        
        return healthToRecover;
    }

    /// <param name="damageText">if there is a text for the damage, then use this. Use $ in the string and it will be replaced with the damage dealt</param>
    /// <param name="caster">The character causing the damage</param>
    /// <param name="damage">The potential damage</param>
    public DamageResult? FixedDamage(DamageArgs damageArgs)
    {
        if (IsDead) return null;
        var damageText = damageArgs.DamageText;
        var damage = damageArgs.Damage;
        var caster = damageArgs.DamageDealer;
        var canBeCountered = damageArgs.CanBeCountered;
        if (damageText is null)
        {
            damageText = $"{this} took $ fixed damage!";
        }
        CurrentBattle.AddAdditionalBattleText(damageText.Replace("$", damage.Round().ToString()));
        TakeDamageWhileConsideringShield(damage.Round());
        DamageResult damageResult;
        if (damageArgs.Move is not null)
        {
            damageResult = new DamageResult(damageArgs.Move)
            {
              
                WasCrit = false,
                Damage = damage.Round(),
                DamageDealer = caster, 
                DamageReceiver = this,
                CanBeCountered = canBeCountered
            };
        }
        else
        {
            damageResult = new DamageResult(damageArgs.StatusEffect)
            {
              
                WasCrit = false,
                Damage = damage.Round(),
                DamageDealer = caster, 
                DamageReceiver = this,
                CanBeCountered = canBeCountered
            };
        }
            
        CurrentBattle.InvokeBattleEvent(new CharacterPostDamageEventArgs(damageResult));
        return damageResult;
    }
        public void TakeDamageWhileConsideringShield(int damage)
    {
        var shield = Shield;

        if (shield is null)
        {
            Health -= damage;
            return;
        }

        var shieldValue = shield.GetShieldValue(this);
        // Check if the shield can absorb some of the damage
        if (shieldValue > 0)
        {
            shieldValue -= damage;
            if (shieldValue < 0)
            {
                Health += shieldValue;
                shieldValue = 0;
            }
        }
        else
        {
            // If no shield, damage affects health directly
            Health -= damage;
        }
        shield.SetShieldValue(shieldValue);
    }
    public static float DamageFormula(float potentialDamage, float defense)
    {
        return (potentialDamage * 375) / (300 + defense);
    }
    /// <summary>
    /// Used to damage this character
    /// </summary>
    /// <param name="damage">The potential damage</param>
    /// <param name="damageText">if there is a text for the damage, then use this. Use $ in the string and it will be replaced with the damage dealt</param>
    /// <param name="caster">The character causing the damage</param>
    /// <param name="canCrit">Whether the damage can cause a critical hit or not</param>
    /// <param name="damageElement">The element of the damage</param>
    /// <returns>The results of the damage</returns>
    public  DamageResult Damage(DamageArgs damageArgs)
    {
        if (IsDead)
        {
            try
            {
                throw new Exception("Attempting to damage dead character");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new DamageResult()
                    { CanBeCountered = false, Damage = 0, DamageDealer = null, DamageReceiver = null };
            }
        }
        CurrentBattle.InvokeBattleEvent(new CharacterPreDamageEventArgs(damageArgs));
        var didCrit = false;
        var defenseToIgnore = Math.Clamp(damageArgs.DefenseToIgnore,0,100);
        var defenseToUse = (100 - defenseToIgnore) * 0.01f * Defense;
        var damageModifyPercentage = 0;
        
        var damage = DamageFormula(damageArgs.Damage, defenseToUse);

        var advantageLevel = ElementalAdvantage.Neutral;
        if(damageArgs.ElementToDamageWith is not null)
            advantageLevel = BattleFunctionality.GetAdvantageLevel(damageArgs.ElementToDamageWith.Value, Element);

        switch (advantageLevel){
            case ElementalAdvantage.Disadvantage:
                damageModifyPercentage -= 30;
                break;
            case ElementalAdvantage.Advantage:
                damageModifyPercentage += 30;
                break;
        }
    


        damage = (damage * 0.01f * (damageModifyPercentage + 100));
        var chance = damageArgs.CriticalChance;
        if (damageArgs.AlwaysCrits)
        {
            chance = 100;
        }
        if (BasicFunctionality.RandomChance(chance) && damageArgs.CanCrit)
        {

            damage *= damageArgs.CriticalDamage / 100.0f;
            didCrit = true;
        }

        var actualDamage = damage.Round();
        var damageText = damageArgs.DamageText;
        if (damageText is null)
        {
            damageText = $"{damageArgs.DamageDealer} dealt {actualDamage} damage to {this}!";
        }

        damageText = damageText.Replace("$", actualDamage.ToString());

        switch (advantageLevel)
        {
            case ElementalAdvantage.Advantage:
                damageText = "It's super effective! " + damageText;
                break;
            case ElementalAdvantage.Disadvantage:
                damageText = "It's not that effective... " + damageText;
                break;
        }
    

        if (didCrit)
            damageText = "A critical hit! " + damageText;

    
        CurrentBattle.AddAdditionalBattleText(damageText);
        
        TakeDamageWhileConsideringShield(actualDamage);
        DamageResult damageResult;
        if (damageArgs.Move is not null)
        {
            damageResult = new DamageResult(damageArgs.Move)
            {
                WasCrit = didCrit,
                Damage = actualDamage,
                DamageDealer = damageArgs.DamageDealer,
                DamageReceiver = this,
                CanBeCountered = damageArgs.CanBeCountered
            };
        }
        else
        {
            damageResult = new DamageResult(damageArgs.StatusEffect)
            {
                
                WasCrit = didCrit,
                Damage = actualDamage,
                DamageDealer = damageArgs.DamageDealer,
                DamageReceiver = this,
                CanBeCountered = damageArgs.CanBeCountered
            };
        }
        CurrentBattle.InvokeBattleEvent(new CharacterPostDamageEventArgs(damageResult));
        return damageResult;
    }
}