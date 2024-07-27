using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class DoNotResist : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Tases the enemy, with a 15% chance to stun for one turn";
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult= target.Damage(new DamageArgs
        {
            DamageSource = new MoveDamageSource()
            {
                Move = this,
                UsageType = usageType
            },
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            DamageDealer = User,
            DamageText = $"{User.NameWithAlphabet} tases {target.NameWithAlphabet} and dealt $ damage! it was shocking"
        });
        if (BasicFunctionality.RandomChance(15))
        {
            target.AddStatusEffect(new Stun(){Duration = 1, Caster = User}, User.Effectiveness);
        }

        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            Text = "DO NOT RESIST!",
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = User,

        };
    }
}

public class IAmShooting : Skill
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Shoots the enemy twice, causing bleed for two turns";
    }
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs
        {
            DamageSource = new MoveDamageSource
            {
                Move = this,
                UsageType = usageType
            },
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageDealer = User,
            Damage = User.Attack * 2,
            DamageText = $"{User.NameWithAlphabet} shoots at {target.NameWithAlphabet} for resisting arrest, dealing $ damage"
        });
 
        target.AddStatusEffect(new Bleed(){ Caster = User, Duration = 2}, User.Effectiveness);
        
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            Text = "I warned you!",
            User = User,
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
        };
    }

    public override int MaxCooldown => 3;
}
public class Police : CharacterPartials.Character
{

    public override Rarity Rarity => Rarity.TwoStar;



    public Police()
    {
        TypeId = 4;
        BasicAttack = new DoNotResist(){User = this};
        Skill = new IAmShooting(){User = this};
     
    }


}