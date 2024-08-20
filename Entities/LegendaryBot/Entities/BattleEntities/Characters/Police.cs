using BasicFunctionality;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;
using Character = Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

using Character = Character;

public class DoNotResist : BasicAttack
{
    public DoNotResist(Character user) : base(user)
    {
    }

    public override string Name => "Do Not Resist";

    public override string GetDescription(Character character)
    {
        return "Tases the enemy, with a 15% chance to stun for one turn";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} tases {target.NameWithAlphabet} and dealt $ damage! it was shocking"
        });
        if (BasicFunctions.RandomChance(15)) target.AddStatusEffect(new Stun(User), User.Effectiveness);


        text = "DO NOT RESIST!";

        attackTargetType = AttackTargetType.SingleTarget;
    }
}

public class IAmShooting : Skill
{
    public IAmShooting(Character user) : base(user)
    {
    }

    public override string Name => "I Am Shooting";

    public override int MaxCooldown => 3;

    public override string GetDescription(Character character)
    {
        return "Shoots the enemy twice, causing bleed for two turns";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 2, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText =
                $"{User.NameWithAlphabet} shoots at {target.NameWithAlphabet} for resisting arrest, dealing $ damage"
        });

        target.AddStatusEffect(new Bleed(User), User.Effectiveness);
        text = "I warned you!";
        attackTargetType = AttackTargetType.SingleTarget;
    }
}

public class Police : Character
{
    public Police()
    {
        Skill = new IAmShooting(this);
        BasicAttack = new DoNotResist(this);
    }

    public override string Name => "Police";
    public override Rarity Rarity => Rarity.TwoStar;


    public override int TypeId
    {
        get => 4;
        protected init { }
    }
}