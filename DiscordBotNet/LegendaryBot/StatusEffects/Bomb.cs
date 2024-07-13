using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Bomb : StatusEffect, IDetonatable
{
    
    public override string Description => "Detonates on the affected when the duration of this status effect finishes." +
                                          " Detonation damage is proportional to the caster's attack. After detonation, the affected gets stunned";
    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public float Attack { get; }
    public Bomb(Character caster) : base(caster)
    {
        Attack = caster.Attack;
    }

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
        Affected.AddStatusEffect(new Stun(detonator));
        return Affected.Damage(        new DamageArgs(this)
        {
            ElementToDamageWith = null,

            Damage = Attack * 3,
            Caster = Caster,
            CanCrit = false,
            DamageText = $"Bomb detonated on {Affected} and dealt $ damage!"
        });
   
    }
    public override bool ExecuteStatusEffectAfterTurn => false;
}