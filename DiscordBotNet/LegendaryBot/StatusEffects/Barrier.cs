using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Barrier : StatusEffect
{
    public override string Description => "Protects the caster with a barrier";
    public override int MaxStacks => 1;
    public override StatusEffectType EffectType => StatusEffectType.Buff;
    private float _shieldValue;
    /// <summary>
    /// using this method makes sure the shield isnt more than the max health
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public float GetShieldValue(Character User)
    {
        var maxHealth = User.MaxHealth;
        if (_shieldValue <= maxHealth)
        {
            return _shieldValue;
        }

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
    public Barrier(Character caster) : base(caster)
    {
        
    }
    public Barrier(Character caster, int shieldValue) : this(caster)
    {
        _shieldValue = shieldValue;
    }

}