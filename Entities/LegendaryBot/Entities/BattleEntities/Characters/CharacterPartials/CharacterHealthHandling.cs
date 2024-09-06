using BasicFunctionality;
using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.Results;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
    /// <summary>
    ///     Recovers the health of this character
    /// </summary>
    /// <param name="toRecover">Amount to recover</param>
    /// <param name="recoveryText">text to say when health recovered. use $ to represent health recovered</param>
    /// <param name="announceHealing"></param>
    /// <returns>Amount recovered</returns>
    public virtual int RecoverHealth(float toRecover,
        string? recoveryText = null, bool announceHealing = true)
    {
        if (CurrentBattle is null)
            throw NoBattleExc;
        if (IsDead) return 0;
        var healthToRecover = toRecover.Round();

        Health += healthToRecover;
        if (recoveryText is null)
            recoveryText = $"{NameWithAlphabet} recovered $ health!";
        if (announceHealing)
            CurrentBattle.AddBattleText(recoveryText.Replace("$", healthToRecover.ToString()));

        return healthToRecover;
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
        return potentialDamage * 375 / (300 + defense);
    }

    /// <summary>
    ///     Used to damage this character
    /// </summary>
    /// <returns>The results of the damage</returns>
    public DamageResult Damage(DamageArgs damageArgs)
    {
        if (CurrentBattle is null)
            throw NoBattleExc;
        
        if (IsDead)
            try
            {
                throw new Exception("Attempting to damage dead character");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new DamageResult
                {
                    CanBeCountered = false, DamageDealt = 0, DamageSource = damageArgs.DamageSource,
                    DamageReceiver = this
                };
            }
        CurrentBattle.InvokeBattleEvent(new CharacterPreDamageEventArgs(damageArgs));
        var damageText = damageArgs.DamageText;
        if (damageText is null) damageText = $"{NameWithAlphabet} took $ damage!";

        var didCrit = false;
        var damage = damageArgs.Damage;
        if (!damageArgs.IsFixedDamage)
        {
            var defenseToIgnore = Math.Clamp(damageArgs.DefenseToIgnore, 0, 100);
            var defenseToUse = (100 - defenseToIgnore) * 0.01f * Defense;
            var damageModifyPercentage = 0;

            var computedDamage = DamageFormula(damageArgs.Damage, defenseToUse);

            var advantageLevel = ElementalAdvantage.Neutral;
            if (damageArgs.ElementToDamageWith is not null)
                advantageLevel = BattleFunctionality.GetAdvantageLevel(damageArgs.ElementToDamageWith.Value, Element);

            switch (advantageLevel)
            {
                case ElementalAdvantage.Disadvantage:
                    damageModifyPercentage -= 30;
                    break;
                case ElementalAdvantage.Advantage:
                    damageModifyPercentage += 30;
                    break;
            }


            computedDamage = computedDamage * 0.01f * (damageModifyPercentage + 100);
            var chance = damageArgs.CriticalChance;
            if (damageArgs.AlwaysCrits) chance = 100;
            if (BasicFunctions.RandomChance(chance) && damageArgs.CanCrit)
            {
                computedDamage *= damageArgs.CriticalDamage / 100.0f;
                didCrit = true;
            }

            damage = computedDamage;
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
        }

        var moveUsageType = (damageArgs.DamageSource as MoveDamageSource)?.MoveUsageContext.MoveUsageType;
        if (moveUsageType is not null)
        {
            if (moveUsageType == MoveUsageType.CounterUsage)
                damageText = "Counter Attack! " + damageText;
            else if (moveUsageType == MoveUsageType.MiscellaneousFollowUpUsage)
                damageText = "Extra Attack! " + damageText;
        }


        damageText = damageText.Replace("$", damage.Round().ToString());
        CurrentBattle.AddBattleText(damageText);
        TakeDamageWhileConsideringShield(damage.Round());
        var damageResult = new DamageResult
        {
            DamageSource = damageArgs.DamageSource,
            IsFixedDamage = damageArgs.IsFixedDamage,
            WasCrit = didCrit,
            DamageDealt = damage,
            DamageReceiver = this,
            CanBeCountered = damageArgs.CanBeCountered
        };
        damageArgs.DamageSource.OnPostDamage(damageResult);
        CurrentBattle.InvokeBattleEvent(new CharacterPostDamageEventArgs(damageResult));
        return damageResult;
    }
}