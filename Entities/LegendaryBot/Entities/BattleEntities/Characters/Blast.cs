using BasicFunctionality;
using DSharpPlus.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class MethaneSlap : BasicAttack
{
    public MethaneSlap(Character user) : base(user)
    {
    }

    public override string Name => "Methane Slap";
    public int DetonateChance => 75;

    public override string GetDescription(Character character)
    {
        return $"Slaps the enemy, " +
               $"producing methane around the enemy, with a " +
               $"{DetonateChance}% chance to detonate all the bombs the target has";
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
            DamageText = $"That was a harsh slap on {target.NameWithAlphabet} dealt $ damage!"
        });
        if (BasicFunctions.RandomChance(DetonateChance))
            foreach (var i in target.StatusEffects.OfType<Bomb>().ToArray())
                i.Detonate(User);
        text = "Methane Slap!";
        attackTargetType = AttackTargetType.SingleTarget;
    }
}

public class BlowAway : Skill
{
    public BlowAway(Character user) : base(user)
    {
    }

    public override string Name => "Blow Away";
    public override int MaxCooldown => 4;

    public int BombInflictChance => 100;

    public override string GetDescription(Character character)
    {
        return
            $"Throws multiple bombs at the enemy, with a {BombInflictChance}% chance each to inflict Bomb status effect";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.BattleTeam != User.BattleTeam && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        User.CurrentBattle!.AddBattleText($"{User.NameWithAlphabet} threw multiple bombs at the opposing team!");
        foreach (var i in GetPossibleTargets())
        foreach (var _ in Enumerable.Range(0, 1))
            if (BasicFunctions.RandomChance(BombInflictChance))
                i.AddStatusEffect(new Bomb(User) { Duration = 2 }, User.Effectiveness);

        text = "Blow Away!";
        attackTargetType = AttackTargetType.AOE;
    }
}

public class ExplosionBlast : Ultimate
{
    public ExplosionBlast(Character user) : base(user)
    {
    }

    public override string Name => "Explosion Blast";


    public override int MaxCooldown => 5;

    public override string GetDescription(Character character)
    {
        return "User does an explosion blast, attacking all enemies, inflicting burn x2 on each enemy hit";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle?.Characters.Where(i => i.BattleTeam != User.BattleTeam && !i.IsDead) ??
               throw Character.NoBattleExc;
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        foreach (var i in GetPossibleTargets())
            i.Damage(new DamageArgs(User.Attack * 1.5f, new MoveDamageSource(moveUsageContext))
            {
                ElementToDamageWith = User.Element,
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                DamageText =
                    $"{User.NameWithAlphabet} blasted {i.NameWithAlphabet}, dealing $ damage!"
            });

        var eff = User.Effectiveness;
        foreach (var i in GetPossibleTargets()) i.AddStatusEffects([new Burn(User), new Burn(User)], eff);

        text = "Blow Away!";
        attackTargetType = AttackTargetType.AOE;
    }
}

public class Blast : Character
{
    public Blast()
    {
        Skill = new BlowAway(this);
        Ultimate = new ExplosionBlast(this);
        BasicAttack = new MethaneSlap(this);
    }

    public override string Name => "Blast";
    public override Rarity Rarity => Rarity.FourStar;
    public override DiscordColor Color => DiscordColor.Brown;

    protected override float BaseSpeedMultiplier => 1.1f;
    protected override float BaseAttackMultiplier => 1.05f;

    public override int TypeId
    {
        get => 18;
        protected init { }
    }
}