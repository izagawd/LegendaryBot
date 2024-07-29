using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Barrier = DiscordBotNet.LegendaryBot.StatusEffects.Barrier;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class ShieldBash : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character) => $"Bashes the shield to the enemy, with a {ShieldStunChanceByBash}% chance to stun"!;


    public int ShieldStunChanceByBash => 10;
    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out TargetType targetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(usageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText =
                $"{User.NameWithAlphabet} bashes {target.NameWithAlphabet} with his shield , making them receive $ damage!",
        });

        targetType = TargetType.SingleTarget;
        text = "Hrraagh!!";

        if (BasicFunctionality.RandomChance(ShieldStunChanceByBash))
        {
            target.AddStatusEffect(new Stun(){Duration = 1, Caster = User});
        }

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
    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out TargetType targetType, out string? text)
    {
        target.AddStatusEffect(new Barrier((ShieldBasedOnDefense * 0.006 * User.Defense).Round())
        {
            Duration = 3,
            Caster = User
        });



        text = $"As a loyal knight, {User.NameWithAlphabet} helps {target.NameWithAlphabet}!";

        targetType = TargetType.SingleTarget;

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
    

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out TargetType targetType, out string? text)
    {
        foreach (var i in GetPossibleTargets())
        {
            i.AddStatusEffect(new DefenseBuff() { Duration = 2, Caster = User});
            
        }


        targetType = TargetType.AOE;
        text = $"As a loyal knight, {User.NameWithAlphabet} increases the defense of all allies for three turns";
;
    }
}

public class RoyalKnight : CharacterPartials.Character
{

    protected override float BaseAttackMultiplier => 0.85f;
    protected override float BaseMaxHealthMultiplier => 1.125f;
    protected override float BaseDefenseMultiplier => 1.15f;
    public override DiscordColor Color => DiscordColor.Blue;
    public override Element Element => Element.Ice;

    
    public RoyalKnight()
    {
        TypeId = 5;
        BasicAttack = new ShieldBash(){User = this};
        Ultimate = new IWillProtectUs(){User = this};
        Skill = new IWillBeYourShield(){User = this};
     
    }

    public override Rarity Rarity => Rarity.ThreeStar;

}
