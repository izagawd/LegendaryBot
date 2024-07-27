using DiscordBotNet.LegendaryBot.ModifierInterfaces;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class DefenseBuff: StatusEffect, IStatsModifier
{

    public override bool IsStackable => false;
   



    public override StatusEffectType EffectType => StatusEffectType.Buff;



    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return
            new DefensePercentageModifierArgs()
            {
                CharacterToAffect = Affected,
                ValueToChangeWith = 50,
       
            };
    }
}