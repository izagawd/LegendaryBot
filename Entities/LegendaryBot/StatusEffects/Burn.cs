using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Results;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.StatusEffects;

public class Burn : StatusEffect, IDetonatable
{
    private float _characterAttack;

    public Burn(CharacterPartials_Character caster) : base(caster)
    {
    }

    public override string Name => "Burn";

    public override string Description =>
        "Does damage at the start of the affected's turn. Damage ignores 70% of defense";

    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override bool ExecuteStatusEffectAfterTurn => false;

    public override bool IsStackable => true;

    public DamageResult? Detonate(CharacterPartials_Character detonator)
    {
        Affected.RemoveStatusEffect(this);
        return DoDamage();
    }


    public override void OnAdded()
    {
        base.OnAdded();
        _characterAttack = Caster.Attack;
    }

    public override void PassTurn()
    {
        base.PassTurn();

        DoDamage();
    }

    private DamageResult? DoDamage()
    {
        if (Affected.IsDead) return null;
        return Affected.Damage(new DamageArgs(_characterAttack * 0.6f,
            new StatusEffectDamageSource(this))
        {
            DefenseToIgnore = 70,
            ElementToDamageWith = null,

            CanCrit = false,
            DamageText = $"{Affected} took $ damage from burn!"
        });
    }
}