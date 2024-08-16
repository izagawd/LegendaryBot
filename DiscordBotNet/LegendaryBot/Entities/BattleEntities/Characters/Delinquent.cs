using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class BaseballBatWhack : BasicAttack
{
    public BaseballBatWhack(Character user) : base(user)
    {
    }

    public override string Name => "Baseball Bat Whack";

    public override string GetDescription(Character character)
    {
        return "Swings a baseball bat at the enemy, causing solid  damage";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText =
                $"{User.NameWithAlphabet} whacks {target.NameWithAlphabet} with a baseball bat, dealing $ damage"
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Temeee!";
    }
}

public class DelinquentBeatdown : Skill
{
    public DelinquentBeatdown(Character user) : base(user)
    {
    }

    public override string Name => "Delinquent Beatdown";

    public override int MaxCooldown => 4;

    public override string GetDescription(Character character)
    {
        return "Calls all other delinquents to beat up ally, dealing solid damage";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Uraah!";
        target.Damage(new DamageArgs(User.Attack * 2.5f,
            new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText =
                $"many of {User.NameWithAlphabet}'s  friends beat up {target.NameWithAlphabet}, dealing $ damage"
        });
    }
}

public class Delinquent : Character
{
    public Delinquent()
    {
        Skill = new DelinquentBeatdown(this);
        BasicAttack = new BaseballBatWhack(this);
    }

    public override string Name => "Delinquent";
    public override Element Element => Element.Fire;


    public override Rarity Rarity => Rarity.TwoStar;

    public override int TypeId
    {
        get => 10;
        protected init { }
    }
}