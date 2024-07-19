using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class AttackDebuff : StatusEffect, IStatsModifier
{

    protected AttackDebuff(){}
    public override int MaxStacks => 1;

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