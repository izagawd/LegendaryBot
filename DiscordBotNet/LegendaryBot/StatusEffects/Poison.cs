using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Poison : StatusEffect, IDetonatable
{

    public override string Description => "Deals damage equivalent to 5% of the affected's max health";
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override bool ExecuteStatusEffectAfterTurn => false;



    public override void PassTurn()
    {
        base.PassTurn();

        DoDamage();

    }

    private DamageResult? DoDamage()
    {
        return Affected.Damage(new DamageArgs(this)
        {
            IsFixedDamage = true,
            ElementToDamageWith = null,
            Damage = Affected.MaxHealth * 0.05f,
            DamageDealer = Caster,
            CanCrit = false,
            DamageText =$"{Affected} took $ damage from being poisoned!"
        });
    }
    public DamageResult? Detonate( Character detonator)
    {
        var removed = Affected.RemoveStatusEffect(this);
        if (removed) return DoDamage();
        return null;
    }
}