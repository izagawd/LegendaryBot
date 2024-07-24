using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class WindSlash : Skill
{

    public override string GetDescription(CharacterPartials.Character character) => "Attacks all enemies with a sharp wind";
    

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        List<DamageResult> damageResults = [];
        foreach (var i in GetPossibleTargets())
        {
            var damageResult = i.Damage(new DamageArgs(this)
            {
                ElementToDamageWith = User.Element,
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                DamageDealer = User,
                Damage = User.Attack * 1.8f, 
                DamageText = $"The slash dealt $ damage to {i}!"
            });
            damageResults.Add(damageResult);
        }

        return new UsageResult(this)
        {
            DamageResults = damageResults,
            TargetType = TargetType.AOE,
            User = User,
            Text = "Wind Slash!",
            UsageType = usageType

        };
    }

    public override int MaxCooldown => 2;
}

public class SimpleSlashOfPrecision : BasicAttack
{
    private int BleedChance => 50;
    public override string GetDescription(CharacterPartials.Character character) =>$"Does a simple slash. Always lands a critical hit, with a {BleedChance}% chance to cause bleed for 2 turns";
    

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageDealer = User,
            Damage = User.Attack * 1.7f,
            AlwaysCrits = true
        });
        if (BasicFunctionality.RandomChance(BleedChance))
        {
            target.AddStatusEffect(new Bleed(){Caster = User});
        }
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            TargetType = TargetType.SingleTarget,
            Text = $"{User.NameWithAlphabetIdentifier} does a simple slash to {target.NameWithAlphabetIdentifier}!",
            User = User,
            UsageType = usageType
        };
    }
}
public class ConsecutiveSlashesOfPrecision : Ultimate
{

    public override string GetDescription(CharacterPartials.Character character)
        =>"Slashes the enemy many times, dealing crazy damage. This attack will always deal a critical hit";

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult =target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            CanCrit = true,
            DamageDealer = User,
            Damage = User.Attack * 1.7f *2,
            AlwaysCrits = true,
            DamageText = $"The slash was so precise it dealt $ damage to {target.NameWithAlphabetIdentifier}!",
     
        });

        return new UsageResult(this)
        {
            DamageResults =  [damageResult],
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = User
        };
    }

    public override int MaxCooldown => 5;
}
public class Slasher : CharacterPartials.Character
{
  
    public override Rarity Rarity => Rarity.FourStar;
    public override DiscordColor Color => DiscordColor.Brown;

    public override Element Element => Element.Earth;

    public Slasher()
    {
        TypeId = 6;
        Ultimate = new ConsecutiveSlashesOfPrecision(){User = this};
        Skill = new WindSlash(){User = this};
        BasicAttack = new SimpleSlashOfPrecision(){User = this};
       
    }

}