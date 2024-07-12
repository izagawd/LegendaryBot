using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Barrier = DiscordBotNet.LegendaryBot.StatusEffects.Barrier;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class ShieldBash : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character) => $"Bashes the shield to the enemy, with a {ShieldStunChanceByBash}% chance to stun"!;


    public int ShieldStunChanceByBash => 10;
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var usageResult =  new UsageResult(this)
        {
            DamageResults = 
            [
                target.Damage(new DamageArgs(this)
                {
                    ElementToDamageWith = User.Element,
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    Caster = User,
                    DamageText =
                        $"{User.NameWithAlphabetIdentifier} bashes {target.NameWithAlphabetIdentifier} with his shield , making them receive $ damage!",
                    Damage = User.Attack * 1.7f

                }),
            ],
            User = User,
            TargetType = TargetType.SingleTarget,
            Text = "Hrraagh!!",
            UsageType = usageType

        };
        if (BasicFunctionality.RandomChance(ShieldStunChanceByBash))
        {
            target.AddStatusEffect(new Stun(User));
        }

        return usageResult;
    }
}
public class IWillBeYourShield : Skill
{
    public override int MaxCooldown => 4;

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.Team.Where(i =>!i.IsDead);
    }

  
    public override string GetDescription(CharacterPartials.Character character) => "gives a shield to the target and caster for 3 turns. Shield strength is proportional to the caster's defense";


    public int ShieldBasedOnDefense => 300;
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        target.AddStatusEffect(new Barrier(User, (ShieldBasedOnDefense * 0.01 * User.Defense).Round()){Duration = 3});


        return new UsageResult(this)
        {
            Text = $"As a loyal knight, {User.NameWithAlphabetIdentifier} helps {target.NameWithAlphabetIdentifier}!",
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = User
        };
    }
}

public class IWillProtectUs : Ultimate
{
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.Team.Where(i => !i.IsDead);
    }

    public override int MaxCooldown => 5;
  

    public override string GetDescription(CharacterPartials.Character character) => "Increases the defense of all allies for 3 turns";
    

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        foreach (var i in GetPossibleTargets())
        {
            i.AddStatusEffect(new DefenseBuff(User) { Duration = 2 });
            
        }

        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.AOE,
            Text = $"As a loyal knight, {User.NameWithAlphabetIdentifier} increases the defense of all allies for three turns",
            User = User
        };
    }
}

public class RoyalKnight : CharacterPartials.Character
{
    public override DiscordColor Color => DiscordColor.Blue;
    public override Element Element => Element.Ice;


    public RoyalKnight()
    {
        BasicAttack = new ShieldBash(){User = this};
        Ultimate = new IWillProtectUs(){User = this};
        Skill = new IWillBeYourShield(){User = this};
     
    }

    public override Rarity Rarity => Rarity.ThreeStar;

}
