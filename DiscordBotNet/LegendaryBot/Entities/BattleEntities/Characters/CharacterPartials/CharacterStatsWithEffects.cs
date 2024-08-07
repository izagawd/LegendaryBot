using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;



public partial class Character
{
    
  
    public float Speed
    {
        get
        {
            float percentage = 100;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();

            float flat = 0;

            foreach (var i in modifiedStats.OfType<SpeedPercentageModifierArgs>())
            {
                percentage += i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.OfType<SpeedFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            var newSpeed = TotalSpeed * percentage * 0.01f;
            newSpeed += flat;
            if (newSpeed < 0) newSpeed = 0;
            return newSpeed;
        }
    }

    public float Defense { 
        get
        {
            float percentage = 100;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();

            float flat = 0;

            foreach (var i in modifiedStats.OfType<DefensePercentageModifierArgs>())
            {
                percentage += i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.OfType<DefenseFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            var newDefense = TotalDefense * percentage * 0.01f;
            newDefense += flat;
            if (newDefense < 0) newDefense = 0;
            return newDefense;
        } 
    }


    public float Attack { 
        get     
        {
            float percentage = 100;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();

            float flat = 0;

            foreach (var i in modifiedStats.OfType<AttackPercentageModifierArgs>())
            {
                percentage += i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.OfType<AttackFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            var newAttack = TotalAttack * percentage * 0.01f;
            newAttack += flat;
            if (newAttack < 0) newAttack = 0;
            return newAttack;
        } 
    }


    public float CriticalDamage {
        get
        {
       
            var percentage = TotalCriticalDamage;

            foreach (var i in GetAllStatsModifierArgs<CriticalDamageModifierArgs>())
            {
                percentage += i.ValueToChangeWith;
            }
            return percentage;
        }
    }

 
    [NotMapped]
    public float Effectiveness
    {
        get
        {
       
            var percentage = TotalEffectiveness;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
            foreach (var i in modifiedStats.OfType<EffectivenessModifierArgs>())
            {
                percentage += i.ValueToChangeWith;
            }
            return percentage;
        }
    }

 
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">the type of stats modifier you want</typeparam>
    public IEnumerable<T> GetAllStatsModifierArgs<T>() where T : StatsModifierArgs
    {
       
        if (CurrentBattle is not null)
        {
            return CurrentBattle
                .GetAllStatsModifierArgsInBattle()
                .OfType<T>()
                .Where(i => i.CharacterToAffect == this);
        }

        return [];
    }


    [NotMapped]
    public float CriticalChance {
        get
        {
            var percentage = TotalCriticalChance;
            foreach (var i  in GetAllStatsModifierArgs<CriticalChanceModifierArgs>())
            {
                percentage += i.ValueToChangeWith;
            }
            return percentage;
        }
        
    }
    [NotMapped]
    public float Resistance {
        get
        {
            var percentage = TotalResistance;
            foreach (var i in GetAllStatsModifierArgs<ResistanceModifierArgs>())
            {
                percentage += i.ValueToChangeWith;
            }
            return percentage;
        } 
    }
}