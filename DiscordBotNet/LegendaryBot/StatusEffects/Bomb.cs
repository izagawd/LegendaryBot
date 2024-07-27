using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Bomb : StatusEffect, IDetonatable
{
    public override void OnAdded()
    {
        base.OnAdded();
        Attack = Caster.Attack;
    }

    public override string Description => "Detonates on the affected when the duration of this status effect finishes." +
                                          " Detonation damage is proportional to the caster's attack. After detonation, the affected gets stunned";
    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public float Attack { get; private set; }


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
        Affected.AddStatusEffect(new Stun(){Duration = 1,Caster = detonator});
        return Affected.Damage(        new DamageArgs
        {
            DamageSource = new StatusEffectDamageSource()
            {
                StatusEffect = this
            },
            ElementToDamageWith = null,

            Damage = Attack * 3,
            DamageDealer = Caster,
            CanCrit = false,
            DamageText = $"Bomb detonated on {Affected} and dealt $ damage!"
        });
   
    }
    public override bool ExecuteStatusEffectAfterTurn => false;
}