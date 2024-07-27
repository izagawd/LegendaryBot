using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class AttackBuff : StatusEffect, IStatsModifier
{
    public override bool IsStackable => false;
    public override string Description => "Increases the caster's attack by 50%";




    public override StatusEffectType EffectType => StatusEffectType.Buff;





    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new AttackPercentageModifierArgs
            {
                CharacterToAffect = Affected,
                ValueToChangeWith = 50,
  
            };
    }
}