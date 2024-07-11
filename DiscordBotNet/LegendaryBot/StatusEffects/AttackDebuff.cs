using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class AttackDebuff : StatusEffect, IStatsModifier
{


    public override int MaxStacks => 1;

    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public AttackDebuff( Character caster) : base(caster)
    {

    }




 

    public  IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new AttackPercentageModifierArgs()
        {
            CharacterToAffect = Affected,
            ValueToChangeWith = -50,

        };
    }


}