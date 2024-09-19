using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class CommanderJeanTonfaWhack : BasicAttack
{
    public CommanderJeanTonfaWhack(Character user) : base(user)
    {
    }

    public override string Name => "Commander Jean Tonfa Whack";

    public override string GetDescription(Character character)
    {
        return "Whacks the enemy with a tonfa, increasing super points by 1 point";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType,
        out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
        {
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} whacks {target.NameWithAlphabet} with her tonfa, dealing $ damage"
        });
        text = "Stand Down!";
        attackTargetType = AttackTargetType.SingleTarget;
        User.SuperPoints++;
    }
}


public class CommanderJeanGrenade : Ultimate
{
    public CommanderJeanGrenade(Character user) : base(user)
    {
    }

    public override string Name => "Commander Jean Grenade";

    public override int MaxCooldown => 6;

    public override string GetDescription(Character character)
    {
        return
            "Throws multiple grenades at the enemy, dealing damage, and inflicting 2 bleed effects for 2 turns, increasing super points by 3";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => !i.IsDead && i.BattleTeam != User.BattleTeam);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType,
        out string? text)
    {
        foreach (var i in GetPossibleTargets())
            i.Damage(new DamageArgs(User.Attack * 2f, new MoveDamageSource(moveUsageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                DamageText =
                    $"{User.NameWithAlphabet} threw a grenade which exploded at {target.NameWithAlphabet}, dealing $ damage!"
            });
        foreach (var i in GetPossibleTargets())
            i.AddStatusEffects([
                new Bleed(User)
                {
                    Duration = 2
                },
                new Bleed(User)
                {
                    Duration = 2
                }
            ], User.Effectiveness);

        attackTargetType = AttackTargetType.AOE;
        User.SuperPoints += 3;
        text = "Grenade!";
    }
}

public class CommanderJeanFiringSquad : Skill
{
    public override bool IsPassive => true;

    public CommanderJeanFiringSquad(Character user) : base(user)
    {
    }

    public override string Name => "Commander Jean Firing Squad";

    public override string GetDescription(Character character)
    {
        return "When 5 superpoints are reached, makes a firing squad attack all enemies! this resets superpoints to 0";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle!.Characters.Where(i => i.BattleTeam != User.BattleTeam && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType,
        out string? text)
    {
        User.SuperPoints -= 5;

        foreach (var i in GetPossibleTargets())
            i.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                DamageText = $"{i.NameWithAlphabet} took $ damage from {User.NameWithAlphabet}'s firing squad"
            });
        text = null;
        attackTargetType = AttackTargetType.AOE;
    }

    public override int MaxCooldown => 0;
}

public class CommanderJean : Character
{
    public CommanderJean()
    {
        Skill = new CommanderJeanFiringSquad(this);
        Ultimate = new CommanderJeanGrenade(this);
        BasicAttack = new CommanderJeanTonfaWhack(this);

    }

    protected override float BaseAttackMultiplier => 1.15f;
    public override string Name => "C. Jean";

    public override bool UsesSuperPoints => true;

    protected override float BaseSpeedMultiplier => 1.1f;



    public override Rarity Rarity => Rarity.FiveStar;



    public override int TypeId
    {
        get => 15;
        protected init { }
    }

    [BattleEventListenerMethod]
    public void InvokeFiringSquad(CharacterPostUseMoveEventArgs moveEventArgs)
    {
        if (CannotDoAnything) return;
        if (SuperPoints >= 5)
        {
            var possibleTarget = Skill!.GetPossibleTargets().FirstOrDefault();
            if (possibleTarget is not null)
                Skill!.Utilize(possibleTarget, MoveUsageType.MiscellaneousFollowUpUsage);
        }
    }
}