using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
    
    public void AddStatusEffects(IEnumerable<StatusEffect> statusEffects, float? effectiveness)
    {

        foreach (var i in statusEffects)
        {
            AddStatusEffect(i, effectiveness);
        }
        
    }

    public static bool TryToResist(float effectiveness, float resistance)
    {
        var percentToResistance =resistance -effectiveness;
        if (percentToResistance < 0) percentToResistance = 0;
        return BasicFunctionality.RandomChance(percentToResistance);

    }
    /// <param name="statusEffect">The status effect to add</param>
    /// <param name="effectiveness">the effectiveness of the caster. Null to ignore effect resistance</param>
    /// <returns>true if the status effect was successfully added</returns>
    public StatusEffectInflictResult AddStatusEffect(StatusEffect statusEffect,float? effectiveness)
    {
        var inflictResult = StatusEffectInflictResult.Failed;
        if (statusEffect is null) return StatusEffectInflictResult.Failed;
        if (IsDead) return StatusEffectInflictResult.Failed;
        var arrayOfType =
            _statusEffects.Where(i => i.GetType() == statusEffect.GetType())
                .ToArray();
        statusEffect.Affected = this;
        if (statusEffect.IsStackable || !arrayOfType.Any())
        {
            var added = false;
            if (effectiveness is not null && statusEffect.EffectType == StatusEffectType.Debuff)
            {
                if (!TryToResist(effectiveness.Value,Resistance))
                {
                    added = _statusEffects.Add(statusEffect);
                }
                else
                {
                    inflictResult = StatusEffectInflictResult.Resisted;
                }
                
            }
            else
            {
                added = _statusEffects.Add(statusEffect);
                
            }

            if (added)
                inflictResult = StatusEffectInflictResult.Succeeded;
            CurrentBattle.AddBattleText(new StatusEffectInflictBattleText([this],inflictResult, [statusEffect]));
            if (added)
            {
                statusEffect.OnAdded();
                CurrentBattle.InvokeBattleEvent(new CharacterStatusEffectAppliedEventArgs
                {
                    AddedStatusEffect = statusEffect
                });
            }
            return inflictResult;


        }
        
        if (!statusEffect.IsStackable && arrayOfType.Any())
        {
            void DoOptimize()
            {
                var onlyStatus = arrayOfType.First();
                var optimizedOne = onlyStatus.OptimizeWith(statusEffect);
                _statusEffects.Remove(onlyStatus);
                _statusEffects.Add(optimizedOne);
                inflictResult = StatusEffectInflictResult.Optimized;
                CurrentBattle.AddBattleText(new StatusEffectInflictBattleText([this],inflictResult, [optimizedOne]));
            }
            if (statusEffect.EffectType == StatusEffectType.Debuff && effectiveness is not null)
            {
                if (!TryToResist(effectiveness.Value, Resistance))
                {
                    DoOptimize();
                }
                else
                {
                    inflictResult = StatusEffectInflictResult.Resisted;
                }
            }
            else
            {
                DoOptimize();
            }
            return inflictResult;
        }
        inflictResult = StatusEffectInflictResult.Failed;
        CurrentBattle.AddBattleText(new StatusEffectInflictBattleText([this],inflictResult, [statusEffect]));
        return inflictResult;
    }
    /// <summary>
    /// Dispells (removes) a debuff from the character
    /// </summary>
    /// <param name="statusEffect">The status effect to remove</param>
    /// <param name="effectiveness">If not null, will do some rng based on effectiveness to see whether or not to dispell debuff</param>
    /// <returns>true if status effect was successfully dispelled</returns>
    public bool DispellStatusEffect(StatusEffect statusEffect, int? effectiveness = null)
    {
        if (effectiveness is null || statusEffect.EffectType == StatusEffectType.Debuff)
            return _statusEffects.Remove(statusEffect);

        if (!BattleFunctionality.CheckForResist(effectiveness.Value,Resistance))
        {
            return _statusEffects.Remove(statusEffect);
        }
        return false;
        
    }
}