using Entities.LegendaryBot.ModifierInterfaces;
using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.StatusEffects;

public class DefenseDebuff : StatusEffect, IStatsModifier
{
    public DefenseDebuff(Character caster) : base(caster)
    {
    }

    public override string Name => "Defense Debuff";
    public override bool IsStackable => false;


    public override StatusEffectType EffectType => StatusEffectType.Debuff;


    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new DefensePercentageModifierArgs(Affected, -50);
    }
}