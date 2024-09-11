using BasicFunctionality;
using DSharpPlus.Entities;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;
using Entities.Models;
using PublicInfo;
using Character = Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

using Character = Character;

public class FourthWallBreaker : BasicAttack
{
    public FourthWallBreaker(Character user) : base(user)
    {
    }

    public override string Name => "Fourth Wall Breaker";

    public override string GetDescription(Character character)
    {
        return "Damages the enemy by breaking the fourth wall";
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
                $"Breaks the fourth wall, causing {target.NameWithAlphabet} to cringe, and making them receive $ damage!"
        });

        attackTargetType = AttackTargetType.SingleTarget;
        text = "It's the power of being a real human";
    }
}

public class FireBall : Skill
{
    public FireBall(Character user) : base(user)
    {
    }

    public override string Name => "Fire Ball";

    public override int MaxCooldown => 2;

    public override string GetDescription(Character character)
    {
        return "Throws a fire ball at the enemy with a 40% chance to inflict burn";
    }


    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.BattleTeam != User.BattleTeam && !i.IsDead);
    }


    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        var damageResult = target.Damage(new DamageArgs(User.Attack * 2.4f, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} threw a fireball at {target.NameWithAlphabet} and dealt $ damage!"
        });
        if (BasicFunctions.RandomChance(10)) target.AddStatusEffect(new Burn(User), User.Effectiveness);

        attackTargetType = AttackTargetType.SingleTarget;
        text = null;
    }
}

public class Ignite : Ultimate
{
    public Ignite(Character user) : base(user)
    {
    }

    public override string Name => "Ignite";
    public override int MaxCooldown => 4;

    public override string GetDescription(Character character)
    {
        return "Ignites the enemy with 3 burns for 2 turns!";
    }


    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.BattleTeam != User.BattleTeam && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet} " +
                                         $"attempts to make a human torch out of {target.NameWithAlphabet}!");

        List<StatusEffect> burns = [];
        for (var i = 0; i < 3; i++) burns.Add(new Burn(User));
        target.AddStatusEffects(burns, User.Effectiveness);
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Ignite!";
    }
}

public class Player : Character
{
    public static int OriginalTypeId = TypesFunction.GetDefaultObject<Player>().TypeId;
    public Player()
    {
        Element = Element.Fire;
        Skill = new FireBall(this);
        Ultimate = new Ignite(this);
        BasicAttack = new FourthWallBreaker(this);
    }

    public override DiscordColor Color => UserData?.Color ?? base.Color;
    public override bool CanSpawnNormally => false;

    protected override IEnumerable<StatType> LevelMilestoneStatIncrease =>
    [
        StatType.Attack, StatType.Attack, StatType.Speed, StatType.CriticalChance, StatType.CriticalDamage,
        StatType.Speed
    ];

    public override bool CanBeTraded => false;
    protected override float BaseAttackMultiplier => 1.05f;

    public override Rarity Rarity => Rarity.FiveStar;


    protected override float BaseSpeedMultiplier => 1.1f;


    public override int TypeId
    {
        get => 1;
        protected init { }
    }

    public override string ImageUrl => GetImageUrl(UserData?.Gender ?? Gender.Male);


    public override string Name => UserData?.Name ?? "Player";

    public static string GetImageUrl(Gender gender)
    {
        return gender == Gender.Male
            ? $"{Information.ApiDomainName}/battle_images/characters/PlayerMale.png"
            : $"{Information.ApiDomainName}/battle_images/characters/PlayerFemale.png";
    }


    public void SetElement(Element element)
    {
        Element = element;
    }
}