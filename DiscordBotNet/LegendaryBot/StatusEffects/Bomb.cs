using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Bomb : StatusEffect, IDetonatable
{
    public override string Name => "Bomb";
    public override void OnAdded()
    {
        base.OnAdded();
        Attack = Caster.Attack;
    }

    public override string Description => "Detonates on the affected when the duration of this status effect finishes." +
                                          " Detonation damage is proportional to the caster's attack. After detonation, the affected gets stunned";
    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public float Attack { get; private set; }

    public override bool IsStackable => true;
    public override void PassTurn()
    {
        base.PassTurn();
        if (Duration == 0)
        {
            Detonate(Caster);
        }
    }

    public DamageResult? Detonate( Character detonator)
    {
        Affected.RemoveStatusEffect(this);
        Affected.AddStatusEffect(new Stun(detonator), detonator.Effectiveness);
        return Affected.Damage(        new DamageArgs(Attack * 3,new StatusEffectDamageSource(this) )
        {
            ElementToDamageWith = null,
            CanCrit = false,
            DamageText = $"Bomb detonated on {Affected} and dealt $ damage!"
        });
   
    }
    public override bool ExecuteStatusEffectAfterTurn => false;

    public Bomb(Character caster) : base(caster)
    {
    }
}