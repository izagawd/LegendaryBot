using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class ThugPunch : BasicAttack
{
    public ThugPunch(Character user) : base(user)
    {
    }

    public override string Name => "Thug Punch";

    public override string GetDescription(Character character)
    {
        return "Punches the enemy in a thug way";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.5f, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText =
                $"{User.NameWithAlphabet} punches {target.NameWithAlphabet} with terrible battle stance, dealing $ damage!"
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Uraah!";
    }
}

public class ThugInsult : Skill
{
    public ThugInsult(Character user) : base(user)
    {
    }

    public override string Name => "Thug Insult";

    public override int MaxCooldown => 4;

    public override string GetDescription(Character character)
    {
        return "Insults the enemy, decreasing defense for 2 turns";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType,
        out string? text)
    {
        CurrentBattle.AddBattleText($"{User.NameWithAlphabet} insults {target.NameWithAlphabet} like a thug!");
        target.AddStatusEffect(new DefenseDebuff(User) { Duration = 2 }, User.Effectiveness);
        attackTargetType = AttackTargetType.SingleTarget;
        text = "What are you gonna do about it?";
    }
}

public class Thug : Character
{
    public Thug()
    {
        BasicAttack = new ThugPunch(this);
        Skill = new ThugInsult(this);
    }

    public override string Name => "Thug";


    public override Rarity Rarity => Rarity.TwoStar;

    public override int TypeId
    {
        get => 9;
        protected init { }
    }
}