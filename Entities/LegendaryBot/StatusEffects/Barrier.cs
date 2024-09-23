using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.StatusEffects;

public class Barrier : StatusEffect
{
    private float _shieldValue;

    public Barrier(Character caster) : this(caster, 0)
    {
    }

    public Barrier(Character caster, int shieldValue) : base(caster)
    {
        _shieldValue = shieldValue;
    }

    public override string Name => "Barrier";
    public override string Description => "Protects the caster with a barrier";
    public override bool IsStackable => false;
    public override StatusEffectType EffectType => StatusEffectType.Buff;

    /// <summary>
    ///     using this method makes sure the shield isnt more than the max health
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public float GetShieldValue(Character User)
    {
        var maxHealth = User.MaxHealth;
        if (_shieldValue <= maxHealth) return _shieldValue;

        _shieldValue = maxHealth;
        return maxHealth;
    }

    public void SetShieldValue(float value)
    {
        if (value <= 0)
        {
            value = 0;
            Affected.RemoveStatusEffect(this);
        }

        _shieldValue = value;
    }
}