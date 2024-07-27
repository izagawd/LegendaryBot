using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class AttackDebuff : StatusEffect, IStatsModifier
{

    public AttackDebuff(){}
    public override bool IsStackable => false;

    public override StatusEffectType EffectType => StatusEffectType.Debuff;




 

    public  IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new AttackPercentageModifierArgs()
        {
            CharacterToAffect = Affected,
            ValueToChangeWith = -50,

        };
    }


}