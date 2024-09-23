using Entities.LegendaryBot.ModifierInterfaces;
using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.StatusEffects;

public class DefenseBuff : StatusEffect, IStatsModifier
{
    public DefenseBuff(Character caster) : base(caster)
    {
    }

    public override string Name => "Defense Buff";
    public override bool IsStackable => false;


    public override StatusEffectType EffectType => StatusEffectType.Buff;


    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new DefensePercentageModifierArgs(Affected, 50);
    }
}