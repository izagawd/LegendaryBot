using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class RoxyBatWhack : BasicAttack
{

    public int SkillUseChance => 50;
    public override string GetDescription(Character character)
    {
        return $"Whacks the target with a bat, with a {SkillUseChance}% chance to use their skill";
    }

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType,
        out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(usageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            DamageText = $"{User.NameWithAlphabet} whacks {target.NameWithAlphabet} with her bat very hard, dealing $ damage!"
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Dont get in my way!";
    }   
}


public class RoxyAggressiveOverload : Skill
{
    public override string GetDescription(Character character)
    {
        return "Aggressively whacks the target with their bat non stop!";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => !i.IsDead && i.Team != User.Team);
    }

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType,
        out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 2.5f, new MoveDamageSource(usageContext))
        {
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            ElementToDamageWith = User.Element,
            DamageText = $"{User.NameWithAlphabet} aggressively whacks {target.NameWithAlphabet} with their bat non stop, dealing $ damage!\nits so brutal to watch..."
        });
        text = "HRRAAH";
        attackTargetType = AttackTargetType.SingleTarget;
    }

    public override int MaxCooldown { get; }
}

public class RoxyHeadBatWhack : Ultimate
{
    public override string GetDescription(Character character)
    {
        return "Caster whacks enemy on the head, dealing increadible damage, stunning them for 1 turn";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType,
        out string? text)
    {
        target.Damage(
            new DamageArgs(target.Attack * 1.7f * 2, new MoveDamageSource(usageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                ElementToDamageWith = User.Element,
                DamageText = $"{User.NameWithAlphabet} whacks {target.NameWithAlphabet} in the head, dealing $ damage.. ouch"
            });
        text = "Can you move after THIS??";
        attackTargetType = AttackTargetType.SingleTarget;
    }

    public override int MaxCooldown { get; }
}
public class Roxy : Character
{
    public override Rarity Rarity => Rarity.FourStar;

    public Roxy()
    {
        TypeId = 14;
        Skill = new RoxyAggressiveOverload();
        BasicAttack = new RoxyBatWhack();
        Ultimate = new RoxyHeadBatWhack();
    }

    protected override float BaseDefenseMultiplier => 1000;
    protected override float BaseSpeedMultiplier => 100;

    [BattleEventListenerMethod]
    public void ShouldProcSkill(CharacterPostUseMoveEventArgs postUseMoveEventArgs)
    {
        if(CannotDoAnything) return;
        if(postUseMoveEventArgs.User != this) return;
        if (postUseMoveEventArgs.Move is RoxyBatWhack roxyBatWhack)
        {
           
            var damageResult = postUseMoveEventArgs.DamageResults.First();
            if(damageResult.DamageReceiver.IsDead) return;
            if(BasicFunctionality.RandomChance(roxyBatWhack.SkillUseChance))
            {
                Skill!.Utilize(damageResult.DamageReceiver, MoveUsageType.MiscellaneousFollowUpUsage);
            }
           
        }
        
    }
}