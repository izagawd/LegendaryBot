using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Results;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.StatusEffects;

public class Bomb : StatusEffect, IDetonatable
{
    public Bomb(CharacterPartials_Character caster) : base(caster)
    {
    }

    public override string Name => "Bomb";

    public override string Description =>
        "Detonates on the affected when the duration of this status effect finishes." +
        " Detonation damage is proportional to the caster's attack. After detonation, the affected gets stunned";

    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public float Attack { get; private set; }

    public override bool IsStackable => true;
    public override bool ExecuteStatusEffectAfterTurn => false;

    public DamageResult? Detonate(CharacterPartials_Character detonator)
    {
        Affected.RemoveStatusEffect(this);
        Affected.AddStatusEffect(new Stun(detonator), detonator.Effectiveness);
        return Affected.Damage(new DamageArgs(Attack * 3, new StatusEffectDamageSource(this))
        {
            ElementToDamageWith = null,
            CanCrit = false,
            DamageText = $"Bomb detonated on {Affected} and dealt $ damage!"
        });
    }

    public override void OnAdded()
    {
        base.OnAdded();
        Attack = Caster.Attack;
    }

    public override void PassTurn()
    {
        base.PassTurn();
        if (Duration == 0) Detonate(Caster);
    }
}