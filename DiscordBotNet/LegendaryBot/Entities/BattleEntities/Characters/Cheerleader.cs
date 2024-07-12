using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class PomPomAttack : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Caster hits the enemy with a pom-pom... and that it";
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {

        return new UsageResult(this)
        {
            DamageResults =
            [
                target.Damage(new DamageArgs(this)
                {
                    ElementToDamageWith = User.Element,
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    Caster = User,
                    Damage = User.Attack * 0.8f,
                    DamageText = $"{User.NameWithAlphabetIdentifier} hits {target.NameWithAlphabetIdentifier} with their pompoms, dealing $ damage!"
                })
            ],
            TargetType = TargetType.SingleTarget,
            User = User,
            UsageType = usageType
        };
    }
}

public class  YouCanDoIt : Skill
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Increases the combat readiness of a single target by 100%, increasing their attack for 2 turns. " +
               "Cannot be used on self";
    }

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.Team.Where(i => !i.IsDead && i != User);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabetIdentifier} wants {target.NameWithAlphabetIdentifier} to prevail!");
        target.IncreaseCombatReadiness(100);
        target.AddStatusEffect(new AttackBuff(User) { Duration = 2 });

        return new UsageResult(this)
        {
            TargetType = TargetType.SingleTarget,
            User = User,
            UsageType = usageType,
        };
    }
    public override int MaxCooldown => 2;
}

public class YouCanMakeItEveryone : Ultimate
{
    private int CombatIncreaseAmount => 30;
    public override string GetDescription(CharacterPartials.Character character)
    {
        return $"Encourages all allies, increasing their combat readiness by {CombatIncreaseAmount}%, and increases their attack for 2 turns";
    }

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.Team.Where(i => !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabetIdentifier} encourages her allies!");

        var targets = GetPossibleTargets().ToArray();

        using (User.CurrentBattle.PauseBattleEventScope)
        {
            foreach (var i in targets)
            {
                i.IncreaseCombatReadiness(CombatIncreaseAmount);
            }
            foreach (var i in targets)
            {
                i.AddStatusEffect(new AttackBuff(User) { Duration = 2 });
            }

        }

        return new UsageResult(this)
        {
            TargetType = TargetType.AOE,
            User = User,
            UsageType = UsageType.NormalUsage
        };

    }

    public override int MaxCooldown => 4;
}
public class Cheerleader : CharacterPartials.Character
{




    public Cheerleader()
    {
        BasicAttack = new PomPomAttack(){User = this};
        Skill = new YouCanDoIt(){User = this};
        Ultimate = new YouCanMakeItEveryone(){User = this};
       
    }

}