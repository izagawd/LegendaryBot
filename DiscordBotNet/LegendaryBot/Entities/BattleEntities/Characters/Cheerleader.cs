using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class PomPomAttack : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Caster hits the enemy with a pom-pom... and that it";
    }

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out TargetType targetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 0.8f, new MoveDamageSource(usageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} hits {target.NameWithAlphabet} with their pompoms, dealing $ damage!"
        });
        targetType = TargetType.SingleTarget;
        text = null;
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

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, 
        out TargetType targetType, out string? text)
    {
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet} wants {target.NameWithAlphabet} to prevail!");
        target.IncreaseCombatReadiness(100);
        target.AddStatusEffect(new AttackBuff() { Duration = 2 , Caster = User});

        targetType = TargetType.SingleTarget;

        text = "You can do it!";
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

    protected override void UtilizeImplementation(Character target, UsageContext usageContext,
        out TargetType targetType, out string? text)
    {
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet} encourages her allies!");

        var targets = GetPossibleTargets().ToArray();

    
        foreach (var i in targets)
        {
            i.IncreaseCombatReadiness(CombatIncreaseAmount);
        }
        foreach (var i in targets)
        {
            i.AddStatusEffect(new AttackBuff() { Duration = 2, Caster = User});
        }


        targetType = TargetType.AOE;
        text = null;
    }


    public override int MaxCooldown => 4;
}
public class Cheerleader : CharacterPartials.Character
{


    public override Rarity Rarity => Rarity.FourStar;

    public Cheerleader()
    {
        TypeId = 12;
        BasicAttack = new PomPomAttack(){User = this};
        Skill = new YouCanDoIt(){User = this};
        Ultimate = new YouCanMakeItEveryone(){User = this};
       
    }

}