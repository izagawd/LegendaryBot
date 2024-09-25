using BasicFunctionality;
using DSharpPlus.Entities;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;


namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;



public class ChamomileSachetWhack : BasicAttack
{
    public ChamomileSachetWhack(Character user) : base(user)
    {
    }

    public override string Name => "Chamomile Sachet Whack";


    public int SleepChance => 25;

    public override string GetDescription(Character character)
    {
        return
            $"With the power of Chamomile, whacks an enemy with a sack filled with Chamomile, with a {SleepChance}% chance of making the enemy sleep";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            CanCrit = true,
            DamageText = $"That was a harsh snoozy whack that dealt $ damage on {target.NameWithAlphabet}!"
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Chamomile Whack!";
        if (BasicFunctions.RandomChance(SleepChance)) target.AddStatusEffect(new Sleep(User), User.Effectiveness);
    }
}

public class BlossomTouch : Skill
{
    public BlossomTouch(Character user) : base(user)
    {
    }

    public override string Name => "Blossom Touch";
    public override int MaxCooldown => 3;

    public int HealthHealScaling => 30;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.BattleTeam.Where(i => !i.IsDead);
    }

    public override string GetDescription(Character character)
    {
        return
            $"With the power of flowers, recovers the hp of an ally with {HealthHealScaling}% of the caster's max health, dispelling one debuff";
    }


    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.RecoverHealth((User.MaxHealth * HealthHealScaling * 0.01).Round());

        text = $"{User.NameWithAlphabet} used Blossom Touch!";
        attackTargetType = AttackTargetType.None;
    }
}

public class LilyOfTheValley : Ultimate
{
    public LilyOfTheValley(Character user) : base(user)
    {
    }

    public override string Name => "Lily Of The Valley";
    public override int MaxCooldown => 5;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.BattleTeam != User.BattleTeam && !i.IsDead);
    }


    public override string GetDescription(Character character)
    {
        return
            "Releases a poisonous gas to a single enemy,  inflicting stun for 1 turn, and inflicts poison x2 for 2 turns";
    }


    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        User.CurrentBattle.AddBattleText(
            $"{User.NameWithAlphabet} used Lily of The Valley, and released a dangerous gas to {target.NameWithAlphabet}!");

        var effectiveness = User.Effectiveness;

        target.AddStatusEffects([
            new Poison(User) { Duration = 2 },
            new Poison(User) { Duration = 2 }, new Stun(User)
        ], effectiveness);
        text = "The valley!";
        attackTargetType = AttackTargetType.None;
    }
}

public class Lily : Character
{
    public Lily()
    {
        BasicAttack = new ChamomileSachetWhack(this);
        Ultimate = new LilyOfTheValley(this);
        Skill = new BlossomTouch(this);
    }

    public override string Name => "Lily";
    protected override float BaseSpeedMultiplier => 1.15f;


    public override Rarity Rarity => Rarity.FourStar;


    public override DiscordColor Color => DiscordColor.HotPink;

    protected override IEnumerable<StatType> DivineEchoStatIncrease =>
    [
        StatType.MaxHealth, StatType.Defense,
        StatType.Effectiveness, StatType.Effectiveness, StatType.MaxHealth, StatType.Defense
    ];

    protected override float BaseMaxHealthMultiplier => 1.1f;
    protected override float BaseDefenseMultiplier => 1.05f;

    protected override float BaseAttackMultiplier => 0.9f;


    public override int TypeId
    {
        get => 3;
        protected init { }
    }

    public override Element Element => Element.Earth;

    public override void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        if (Ultimate.CanBeUsedNormally())
        {
            decision = BattleDecision.Ultimate;
            target = BasicFunctions.RandomChoice(Ultimate.GetPossibleTargets());
            return;
        }

        var teamMateWithLowestHealth = BattleTeam.OrderBy(i => i.Health).First();
        if (Skill.CanBeUsedNormally() && teamMateWithLowestHealth.Health < teamMateWithLowestHealth.MaxHealth * 0.7)
        {
            decision = BattleDecision.Skill;
            target = teamMateWithLowestHealth;
            return;
        }

        decision = BattleDecision.BasicAttack;

        target = BasicAttack.GetPossibleTargets().OrderBy(i => i.Health).First();
    }
}