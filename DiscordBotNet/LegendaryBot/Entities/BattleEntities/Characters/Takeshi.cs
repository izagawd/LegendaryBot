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
    public override string GetDescription(Character character)
    {
        return "Does a simple but powerful straight punch at the enemy!";
    }

    protected override UsageResult UtilizeImplementation(Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults =
            [
                target.Damage(new DamageArgs
                {
                    DamageSource = new MoveDamageSource()
                    {
                        Move = this,
                        UsageType = usageType
                    },
                    DamageText =
                        $"{User.NameWithAlphabet} does a straight punch at {target.NameWithAlphabet}, dealing $ damage!",
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    ElementToDamageWith = User.Element,
                    Damage = User.Attack * 1.7f,
                    DamageDealer = User
                })
            ],
            TargetType = TargetType.SingleTarget,
            User = User,
            UsageType = usageType
        };
    }
}
public class TakeshiMeditation : Ultimate
{
    public override string GetDescription(Character character)
    {
        return "Meditates, recovering Health proportional to 50% of the caster's max health, and gains attack buff for 2 turns";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        yield return User;
    }

    protected override UsageResult UtilizeImplementation(Character target, UsageType usageType)
    {
        CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabet} meditates!" );
        User.RecoverHealth(User.MaxHealth * 0.5f);
        User.AddStatusEffect(new AttackBuff() { Caster = User, Duration = 2 });
        return new UsageResult(this)
        {
            TargetType = TargetType.SingleTarget,
            User = User,
            UsageType = usageType,
            Text = "Hummmm...."
        };
    }

    public override int MaxCooldown => 4;
}
public class KarateNeckChop : Skill
{
    public override string GetDescription(Character character)
    {
        return "Does a karate chop at the enemies neck, stunning them for 1 turn!";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => i.Team != User.Team);
    }

    protected override UsageResult UtilizeImplementation(Character target, UsageType usageType)
    {
        CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabet} neck chops {target}!" );
        target.AddStatusEffect(new Stun() { Caster = User, Duration = 1 });
        return new UsageResult(this)
        {
            TargetType = TargetType.SingleTarget,
            User = User,
            UsageType = usageType,
            Text = "Neck chop!"
        };
    }

    public override int MaxCooldown => 3;
}
public class Takeshi : Character
{
    public override Rarity Rarity => Rarity.ThreeStar;
    public override string? PassiveDescription => "Has a 25% chance to counter attack with basic attack when any ally is attacked";

    protected override float BaseSpeedMultiplier => 1.05f;

    [BattleEventListenerMethod]
    public void testingListener(BattleEventArgs args)
    {
        if (args is CharacterDeathEventArgs deathEventArgs && deathEventArgs.Killed == this)
        {
            Revive();
            Health = MaxHealth;
        }
    }

    [BattleEventListenerMethod]
    public void ToCounterAttack(CharacterPostUseMoveEventArgs args)
    {
        Bot.Client.GetChannelAsync(1262087698597023937).Result.SendMessageAsync("Done").GetAwaiter().GetResult();
        if(CannotDoAnything) return;
        if(args.UsageResult.User.Team == Team) return;
        var usageResult = args.UsageResult;
        if(usageResult.UsageType == UsageType.CounterUsage) return;
        var damageDealer = usageResult.User;
        if (damageDealer is null || damageDealer.IsDead || damageDealer.Team == Team)
            return;
        
        foreach (var _ in args.UsageResult.DamageResults
                     .Where(i => i.CanBeCountered && i.DamageReceiver.Team == Team))
        {
          
            if (BasicFunctionality.RandomChance(25))
            {
                BasicAttack.Utilize(damageDealer, UsageType.CounterUsage);
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