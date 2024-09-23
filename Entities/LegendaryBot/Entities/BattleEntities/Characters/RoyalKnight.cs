using BasicFunctionality;
using DSharpPlus.Entities;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;
using Character = Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;
using StatusEffects_Barrier = Entities.LegendaryBot.StatusEffects.Barrier;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

using Character = Character;

public class ShieldBash : BasicAttack
{
    public ShieldBash(Character user) : base(user)
    {
    }

    public override string Name => "Shield Bash";


    public int ShieldStunChanceByBash => 10;

    public override string GetDescription(Character character)
    {
        return $"Bashes the shield to the enemy, with a {ShieldStunChanceByBash}% chance to stun"!;
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText =
                $"{User.NameWithAlphabet} bashes {target.NameWithAlphabet} with his shield , making them receive $ damage!"
        });

        attackTargetType = AttackTargetType.SingleTarget;
        text = "Hrraagh!!";

        if (BasicFunctions.RandomChance(ShieldStunChanceByBash))
            target.AddStatusEffect(new Stun(User) { Duration = 1 },
                User.Effectiveness);
    }
}

public class IWillBeYourShield : Skill
{
    public IWillBeYourShield(Character user) : base(user)
    {
    }

    public override string Name => "I Will Be Your Shield!!";
    public override int MaxCooldown => 4;


    public int ShieldBasedOnDefense => 300;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.BattleTeam.Where(i => !i.IsDead);
    }


    public override string GetDescription(Character character)
    {
        return
            "gives a shield to the target and caster for 3 turns. Shield strength is proportional to the caster's defense";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.AddStatusEffect(new StatusEffects_Barrier(User, (ShieldBasedOnDefense * 0.006 * User.Defense).Round())
        {
            Duration = 3
        }, User.Effectiveness);


        text = $"As a loyal knight, {User.NameWithAlphabet} helps {target.NameWithAlphabet}!";

        attackTargetType = AttackTargetType.None;
    }
}

public class IWillProtectUs : Ultimate
{
    public IWillProtectUs(Character user) : base(user)
    {
    }

    public override string Name => "I Will Protect Us!!";

    public override int MaxCooldown => 5;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.BattleTeam?.Where(i => !i.IsDead) ?? [];
    }


    public override string GetDescription(Character character)
    {
        return "Increases the defense of all allies for 3 turns";
    }


    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        foreach (var i in GetPossibleTargets())
            i.AddStatusEffect(new DefenseBuff(User) { Duration = 2 },
                User.Effectiveness);


        attackTargetType = AttackTargetType.AOE;
        text = $"As a loyal knight, {User.NameWithAlphabet} increases the defense of all allies for three turns";
        ;
    }
}

public class RoyalKnight : Character
{
    public RoyalKnight()
    {
        Skill = new IWillBeYourShield(this);
        Ultimate = new IWillProtectUs(this);
        BasicAttack = new ShieldBash(this);
    }

    public override string Name => "Royal Knight";
    protected override float BaseAttackMultiplier => 0.85f;
    protected override float BaseMaxHealthMultiplier => 1.125f;
    protected override float BaseDefenseMultiplier => 1.15f;
    public override DiscordColor Color => DiscordColor.Blue;
    public override Element Element => Element.Ice;

    public override int TypeId
    {
        get => 5;
        protected init { }
    }

    public override Rarity Rarity => Rarity.FourStar;
}