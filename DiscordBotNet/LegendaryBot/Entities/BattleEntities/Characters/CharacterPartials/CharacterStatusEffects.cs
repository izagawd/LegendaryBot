using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
    
    public void AddStatusEffects(IEnumerable<StatusEffect> statusEffects, float? effectiveness ,
        bool announce = true)
    {
  
        var statusEffectsAsArray = statusEffects.ToArray();
        if (!announce)
        {
            foreach (var i in statusEffectsAsArray)
            {
                AddStatusEffect(i, effectiveness, false);
            }
            return;
        }

        List<StatusEffect> resisted = [];
        List<StatusEffect> succeeded = [];
        List<StatusEffect> failed = [];

        foreach (var i in statusEffectsAsArray)
        {
            var result = AddStatusEffect(i, effectiveness, false);
            switch (result)
            {
                case StatusEffectInflictResult.Resisted:
                    resisted.Add(i);
                    break;
                case StatusEffectInflictResult.Succeeded:
                    succeeded.Add(i);
                    break;
                default:
                    failed.Add(i);
                    break;
            }
        }
        
        
        
        if(succeeded.Count > 0)
            CurrentBattle.AddBattleText(new StatusEffectInflictBattleText(this,StatusEffectInflictResult.Succeeded
                ,succeeded.ToArray()));
        if(resisted.Count > 0)
            CurrentBattle.AddBattleText(new StatusEffectInflictBattleText(this,StatusEffectInflictResult.Resisted
                ,resisted.ToArray()));
        if(failed.Count > 0)
            CurrentBattle.AddBattleText(new StatusEffectInflictBattleText(this,StatusEffectInflictResult.Failed
                ,failed.ToArray()));
        
    }
    /// <param name="statusEffect">The status effect to add</param>
    /// <param name="effectiveness">the effectiveness of the caster. Null to ignore effect resistance</param>
    /// <returns>true if the status effect was successfully added</returns>
    public StatusEffectInflictResult AddStatusEffect(StatusEffect statusEffect,float? effectiveness = null, bool announce =  true)
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
                var percentToResistance =Resistance -effectiveness;
                
                if (percentToResistance < 0) percentToResistance = 0;
                if (!BasicFunctionality.RandomChance((int)percentToResistance))
                {
                    added = _statusEffects.Add(statusEffect);
                    
                }
                
            }
            else
            {
                added = _statusEffects.Add(statusEffect);
                
            }
            inflictResult = StatusEffectInflictResult.Resisted;
            if (added)
            {
                inflictResult =  StatusEffectInflictResult.Succeeded;
                statusEffect.OnAdded();
                CurrentBattle.InvokeBattleEvent(new CharacterStatusEffectAppliedEventArgs
                {
                    AddedStatusEffect = statusEffect
                });
            }
                
            if (announce)
            {
                CurrentBattle.AddBattleText(new StatusEffectInflictBattleText(this,inflictResult, statusEffect));
            }
            return inflictResult;


        }
        if (!statusEffect.IsStackable && arrayOfType.Any())
        {
            var onlyStatus = arrayOfType.First();
            var optimizedOne = onlyStatus.OptimizeWith(statusEffect);
            _statusEffects.Remove(onlyStatus);
            _statusEffects.Add(optimizedOne);
            CurrentBattle.AddBattleText(new StatusEffectInflictBattleText(this,StatusEffectInflictResult.Succeeded, optimizedOne));
            return StatusEffectInflictResult.Succeeded;
        }
        inflictResult = StatusEffectInflictResult.Failed;
        if (announce)
        {
            CurrentBattle.AddBattleText(new StatusEffectInflictBattleText(this,inflictResult, statusEffect));
        }
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