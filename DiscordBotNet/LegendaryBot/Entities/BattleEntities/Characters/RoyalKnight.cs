using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Polly.Retry;
using Barrier = DiscordBotNet.LegendaryBot.StatusEffects.Barrier;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class ShieldBash : BasicAttack
{
    public override string Name => "Shield Bash";
    public override string GetDescription(Character character) => $"Bashes the shield to the enemy, with a {ShieldStunChanceByBash}% chance to stun"!;


    public int ShieldStunChanceByBash => 10;
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText =
                $"{User.NameWithAlphabet} bashes {target.NameWithAlphabet} with his shield , making them receive $ damage!",
        });

        attackTargetType = AttackTargetType.SingleTarget;
        text = "Hrraagh!!";

        if (BasicFunctionality.RandomChance(ShieldStunChanceByBash))
        {
            target.AddStatusEffect(new Stun(User){Duration = 1, },
                User.Effectiveness);
        }

    }

    public ShieldBash(Character user) : base(user)
    {
    }
}
public class IWillBeYourShield : Skill
{
    public override string Name => "I Will Be Your Shield!!";
    public override int MaxCooldown => 4;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.Team.Where(i =>!i.IsDead);
    }

  
    public override string GetDescription(Character character) => "gives a shield to the target and caster for 3 turns. Shield strength is proportional to the caster's defense";


    public int ShieldBasedOnDefense => 300;
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.AddStatusEffect(new Barrier(User,(ShieldBasedOnDefense * 0.006 * User.Defense).Round())
        {
            Duration = 3,
            
        }, User.Effectiveness);



        text = $"As a loyal knight, {User.NameWithAlphabet} helps {target.NameWithAlphabet}!";

        attackTargetType = AttackTargetType.None;

    }

    public IWillBeYourShield(Character user) : base(user)
    {
    }
}

public class IWillProtectUs : Ultimate
{
    public override string Name => "I Will Protect Us!!";
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.Team.Where(i => !i.IsDead);
    }

    public override int MaxCooldown => 5;
  

    public override string GetDescription(Character character) => "Increases the defense of all allies for 3 turns";
    

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        foreach (var i in GetPossibleTargets())
        {
            i.AddStatusEffect(new DefenseBuff(User) { Duration = 2, },
                User.Effectiveness);
            
        }


        attackTargetType = AttackTargetType.AOE;
        text = $"As a loyal knight, {User.NameWithAlphabet} increases the defense of all allies for three turns";
;
    }

    public IWillProtectUs(Character user) : base(user)
    {
    }
}

public class RoyalKnight : Character
{
    public override string Name => "Royal Knight";
    protected override float BaseAttackMultiplier => 0.85f;
    protected override float BaseMaxHealthMultiplier => 1.125f;
    protected override float BaseDefenseMultiplier => 1.15f;
    public override DiscordColor Color => DiscordColor.Blue;
    public override Element Element => Element.Ice;

    public override BasicAttack GenerateBasicAttack()
    {
        return new ShieldBash(this);
    }

    public override Ultimate? GenerateUltimate()
    {
        return new IWillProtectUs(this);
    }

    public override Skill? GenerateSkill()
    {
        return new IWillBeYourShield(this);
    }

    public RoyalKnight()
    {
        TypeId = 5;

    }

    public override Rarity Rarity => Rarity.ThreeStar;

}
