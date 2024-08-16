using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Bleed : StatusEffect, IDetonatable
{
    public Bleed(Character caster) : base(caster)
    {
    }

    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public override string Name => "Bleed";
    public override bool IsStackable => true;

    public override string Description =>
        "Does damage proportional to the caster's attack to the affected at the start of the affected's turn." +
        " Ignores 70% of the affecteed's defense";

    public override bool ExecuteStatusEffectAfterTurn => false;
    public float Attack { get; private set; }

    public DamageResult? Detonate(Character detonator)
    {
        var removed = Affected.RemoveStatusEffect(this);
        if (removed) return DoDamage();
        return null;
    }

    private DamageResult? DoDamage()
    {
        return Affected.Damage(new DamageArgs(Attack, new StatusEffectDamageSource(this))
        {
            ElementToDamageWith = null,
            DefenseToIgnore = 70,
            DamageText = $"{Affected} took $ bleed damage!",
            CanCrit = false
        });
    }

    public override void PassTurn()
    {
        base.PassTurn();
        DoDamage();
    }

    public override void OnAdded()
    {
        base.OnAdded();
        Attack = Caster.Attack;
    }
}