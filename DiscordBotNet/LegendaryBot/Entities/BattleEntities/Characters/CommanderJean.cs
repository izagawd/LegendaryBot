using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;


public class CommanderJeanTonfaWhack : BasicAttack
{
    public override string Name => "Commander Jean Tonfa Whack";
    public override string GetDescription(Character character)
    {
        return "Whacks the enemy with a tonfa, increasing super points by 1 point";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
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
public class CommanderJeanTaser : Skill
{
    public override string Name => "Commander Jean Taser";
    public override string GetDescription(Character character)
    {
        return "Tazes the enemy, stunning them for one turn, and increasing super points by 2";
    }

    public override int MaxCooldown => 3;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => !i.IsDead && i.Team != User.Team);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
        out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 2f, new MoveDamageSource(moveUsageContext))
        {
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} tases {target.NameWithAlphabet}, dealing $ damage!"
        });
        target.AddStatusEffect(new Stun()
        {
            Caster = User,
            Duration = 1,
        },User.Effectiveness);
        
        attackTargetType = AttackTargetType.SingleTarget;
        User.SuperPoints += 2;
        text = "I warned you!";
    }
}
public class CommanderJeanGrenade : Ultimate
{
    public override string Name => "Commander Jean Grenade";
    public override string GetDescription(Character character)
    {
        return "Throws multiple grenades at the enemy, dealing damage, and inflicting 2 bleed effects for 2 turns, increasing super points by 3";
    }

    public override int MaxCooldown => 6;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => !i.IsDead && i.Team != User.Team);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
        out string? text)
    {

        foreach (var i in GetPossibleTargets())
        {
            i.Damage(new DamageArgs(User.Attack * 2f, new MoveDamageSource(moveUsageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                DamageText = $"{User.NameWithAlphabet} threw a grenade which exploded at {target.NameWithAlphabet}, dealing $ damage!"
            });
    
        }
        foreach (var i in GetPossibleTargets())
        {
            i.AddStatusEffects([new Bleed()
            {
                Caster = User,
                Duration = 2,
            },new Bleed()
            {
                Caster = User,
                Duration = 2,
            }],User.Effectiveness);
        }
 
        attackTargetType = AttackTargetType.AOE;
        User.SuperPoints += 3;
        text = "Grenade!";
    }
}

public class CommanderJeanFiringSquad : Skill
{
    public override string Name => "Commander Jean Firing Squad";
    public override string GetDescription(Character character)
    {
        return "d";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
        out string? text)
    {
        User.SuperPoints -= 5;
        
        foreach (var i in GetPossibleTargets())
        {
            i.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                DamageText = $"{i.NameWithAlphabet} took $ damage from {User.NameWithAlphabet}'s firing squad"
            });
        }
        text = null;
        attackTargetType = AttackTargetType.AOE;
    }

    public override int MaxCooldown { get; }
}
public class CommanderJean : Character
{
    protected override float BaseAttackMultiplier => 1.15f;
    public override string Name => "C. Jean";

    public override bool UsesSuperPoints => true;

    [BattleEventListenerMethod]
    public void InvokeFiringSquad(CharacterPostUseMoveEventArgs moveEventArgs)
    {
        if(CannotDoAnything) return;
        if (SuperPoints >= 5)
        {
            var possibleTarget = _firingSquad.GetPossibleTargets().FirstOrDefault();
            if(possibleTarget is not null)
                _firingSquad.Utilize(possibleTarget, MoveUsageType.MiscellaneousFollowUpUsage);
        }
    
    }

    protected override float BaseSpeedMultiplier => 1.1f;

    public override string? PassiveDescription =>
        "Once super points reach 5, a firing squad appears, attacking all enemies";

    public override IEnumerable<Move> MoveList
    {
        get
        {
            foreach (var i in base.MoveList)
            {
                yield return i;
            }

            yield return _firingSquad;
        }
    }

    public override Rarity Rarity => Rarity.FiveStar;
    private Move _firingSquad = new CommanderJeanFiringSquad();
    public CommanderJean()
    {
        TypeId = 15;
        BasicAttack = new CommanderJeanTonfaWhack();
        Skill = new CommanderJeanTaser();
        Ultimate = new CommanderJeanGrenade();
    }
}