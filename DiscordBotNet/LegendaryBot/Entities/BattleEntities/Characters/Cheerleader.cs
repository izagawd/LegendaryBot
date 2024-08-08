using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class PomPomAttack : BasicAttack
{
    public override string Name => "Pom Pom Attack";
    public override string GetDescription(Character character)
    {
        return "Caster hits the enemy with a pom-pom... and that it";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 0.8f, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} hits {target.NameWithAlphabet} with their pompoms, dealing $ damage!"
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = null;
    }

    public PomPomAttack(Character user) : base(user)
    {
    }
}

public class  YouCanDoIt : Skill
{
    public override string Name => "You Can Do It";
    public override string GetDescription(Character character)
    {
        return "Increases the combat readiness of a single target by 100%, increasing their attack for 2 turns. " +
               "Cannot be used on self";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.Team.Where(i => !i.IsDead && i != User);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, 
        out AttackTargetType attackTargetType, out string? text)
    {
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet} wants {target.NameWithAlphabet} to prevail!");
        target.IncreaseCombatReadiness(100);
        target.AddStatusEffect(new AttackBuff(User) { Duration = 2 , }, User.Effectiveness);

        attackTargetType = AttackTargetType.SingleTarget;

        text = "You can do it!";
    }
    public override int MaxCooldown => 2;

    public YouCanDoIt(Character user) : base(user)
    {
    }
}

public class YouCanMakeItEveryone : Ultimate
{
    public override string Name => "You Can Make It Everyone";
    private int CombatIncreaseAmount => 30;
    public override string GetDescription(Character character)
    {
        return $"Encourages all allies, increasing their combat readiness by {CombatIncreaseAmount}%, and increases their attack for 2 turns";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.Team.Where(i => !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet} encourages her allies!");



    
        foreach (var i in GetPossibleTargets())
        {
            i.IncreaseCombatReadiness(CombatIncreaseAmount);
        }

        var eff = User.Effectiveness;
        foreach (var i in GetPossibleTargets())
        {
            i.AddStatusEffect(new AttackBuff(User) { Duration = 2, },eff);
        }


        attackTargetType = AttackTargetType.AOE;
        text = null;
    }


    public override int MaxCooldown => 4;

    public YouCanMakeItEveryone(Character user) : base(user)
    {
    }
}
public class Cheerleader : Character
{
    public override string Name => "Cheerleader";
    public override Rarity Rarity => Rarity.FourStar;
    
    public Cheerleader()
    {
        TypeId = 12;
        Skill = new YouCanDoIt(this);
        BasicAttack = new PomPomAttack(this);
        Ultimate = new YouCanMakeItEveryone(this);
    }

    

}