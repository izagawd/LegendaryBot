using BasicFunctionality;
using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class TakeshiStraightPunch : BasicAttack
{
    public TakeshiStraightPunch(Character user) : base(user)
    {
    }

    public override string Name => "Takeshi Straight Punch";

    public override string GetDescription(Character character)
    {
        return "Does a simple but powerful straight punch at the enemy! Also, has a 25% chance to counter attack with it when any ally is attacked";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
        {
            DamageText =
                $"{User.NameWithAlphabet} does a straight punch at {target.NameWithAlphabet}, dealing $ damage!",
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            ElementToDamageWith = User.Element
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = null;
    }
}

public class TakeshiMeditation : Ultimate
{
    public TakeshiMeditation(Character user) : base(user)
    {
    }

    public override string Name => "Takeshi Meditation";

    public override int MaxCooldown => 4;

    public override string GetDescription(Character character)
    {
        return
            "Meditates, recovering Health proportional to 50% of the caster's max health, and gains attack buff for 2 turns";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        yield return User;
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        CurrentBattle.AddBattleText($"{User.NameWithAlphabet} meditates!");
        User.RecoverHealth(User.MaxHealth * 0.5f);
        User.AddStatusEffect(new AttackBuff(User) { Duration = 2 },
            User.Effectiveness);
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Hummmm....";
    }
}

public class KarateNeckChop : Skill
{
    public KarateNeckChop(Character user) : base(user)
    {
    }

    public override string Name => "Karate Neck Chop";

    public override int MaxCooldown => 3;

    public override string GetDescription(Character character)
    {
        return "Does a karate chop at the enemies neck, stunning them for 1 turn!";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => i.BattleTeam != User.BattleTeam);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        CurrentBattle.AddBattleText($"{User.NameWithAlphabet} neck chops {target}!");
        target.AddStatusEffect(new Stun(User) { Duration = 1 },
            User.Effectiveness);

        attackTargetType = AttackTargetType.SingleTarget;

        text = "Neck chop!";
    }
}

public class Takeshi : Character
{
    public Takeshi()
    {
        Ultimate = new TakeshiMeditation(this);
        Skill = new KarateNeckChop(this);
        BasicAttack = new TakeshiStraightPunch(this);
    }

    public override string Name => "Takeshi";
    public override Rarity Rarity => Rarity.FourStar;

    protected override float BaseSpeedMultiplier => 1.05f;


    public override int TypeId
    {
        get => 13;
        protected init { }
    }


    [BattleEventListenerMethod]
    public void ToCounterAttack(CharacterPostUseMoveEventArgs args)
    {
        if (CannotDoAnything) return;
        if (args.MoveUsageResult.User.BattleTeam == BattleTeam) return;
        var usageResult = args.MoveUsageResult;
        if (usageResult.MoveUsageType == MoveUsageType.CounterUsage) return;
        var damageDealer = usageResult.User;
        if (damageDealer is null || damageDealer.IsDead || damageDealer.BattleTeam == BattleTeam)
            return;

        foreach (var _ in args.MoveUsageResult.DamageResults
                     .Where(i => i.CanBeCountered && i.DamageReceiver.BattleTeam == BattleTeam))
            if (BasicFunctions.RandomChance(25))
            {
                BasicAttack.Utilize(damageDealer, MoveUsageType.CounterUsage);
                break;
            }
    }
}