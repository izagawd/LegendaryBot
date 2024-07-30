using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class DefenseDebuff: StatusEffect, IStatsModifier
{
    public override string Name => "Defense Debuff";
    public override bool IsStackable => false;

   


    public override StatusEffectType EffectType => StatusEffectType.Debuff;

 
    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return
            new DefensePercentageModifierArgs
            {
                CharacterToAffect = Affected,
                ValueToChangeWith = -50,

            };

    }
}