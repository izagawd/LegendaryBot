using System.Net;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class TakeshiStraightPunch : BasicAttack
{
    public override string Name => "Takeshi Straight Punch";
    public override string GetDescription(Character character)
    {
        return "Does a simple but powerful straight punch at the enemy!";
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
            ElementToDamageWith = User.Element,
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = null;


    }
}
public class TakeshiMeditation : Ultimate
{
    public override string Name => "Takeshi Meditation";
    public override string GetDescription(Character character)
    {
        return "Meditates, recovering Health proportional to 50% of the caster's max health, and gains attack buff for 2 turns";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        yield return User;
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        CurrentBattle.AddBattleText($"{User.NameWithAlphabet} meditates!" );
        User.RecoverHealth(User.MaxHealth * 0.5f);
        User.AddStatusEffect(new AttackBuff() { Caster = User, Duration = 2 },
            User.Effectiveness);
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Hummmm....";
    }

    public override int MaxCooldown => 4;
}
public class KarateNeckChop : Skill
{
    public override string Name => "Karate Neck Chop";
    public override string GetDescription(Character character)
    {
        return "Does a karate chop at the enemies neck, stunning them for 1 turn!";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => i.Team != User.Team);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        CurrentBattle.AddBattleText($"{User.NameWithAlphabet} neck chops {target}!" );
        target.AddStatusEffect(new Stun() { Caster = User, Duration = 1 },
            User.Effectiveness);

        attackTargetType = AttackTargetType.SingleTarget;

        text = "Neck chop!";
    }

    public override int MaxCooldown => 3;
}
public class Takeshi : Character
{
    public override string Name => "Takeshi";
    public override Rarity Rarity => Rarity.ThreeStar;
    public override string? PassiveDescription => "Has a 25% chance to counter attack with basic attack when any ally is attacked";

    protected override float BaseSpeedMultiplier => 1.05f;

    
    [BattleEventListenerMethod]
    public void ToCounterAttack(CharacterPostUseMoveEventArgs args)
    {
        if(CannotDoAnything) return;
        if(args.MoveUsageResult.User.Team == Team) return;
        var usageResult = args.MoveUsageResult;
        if(usageResult.MoveUsageType == MoveUsageType.CounterUsage) return;
        var damageDealer = usageResult.User;
        if (damageDealer is null || damageDealer.IsDead || damageDealer.Team == Team)
            return;
        
        foreach (var _ in args.MoveUsageResult.DamageResults
                     .Where(i => i.CanBeCountered && i.DamageReceiver.Team == Team))
        {
          
            if (BasicFunctionality.RandomChance(25))
            {
                BasicAttack.Utilize(damageDealer, MoveUsageType.CounterUsage);
                break;
            }
        }
       

    }

    public Takeshi()
    {
        TypeId = 13;
        BasicAttack = new TakeshiStraightPunch();
        Skill = new KarateNeckChop();
        Ultimate = new TakeshiMeditation();
    }
}