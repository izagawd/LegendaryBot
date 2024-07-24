using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class WindSlash : Skill
{
    private const int increasedCritChance = 25;
    public override string GetDescription(CharacterPartials.Character character) => "Attacks all enemies with a sharp wind." +
        $" Attack has an increased crit chance of {increasedCritChance}%";
    

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        List<DamageResult> damageResults = [];
        foreach (var i in GetPossibleTargets())
        {
            var damageResult = i.Damage(new DamageArgs(this, usageType)
            {
                ElementToDamageWith = User.Element,
                CriticalChance = User.CriticalChance + increasedCritChance,
                CriticalDamage = User.CriticalDamage,
                DamageDealer = User,
                Damage = User.Attack * 1.7f, 
                DamageText = $"The slash dealt $ damage to {i}!"
            });
            if(damageResult is not null)
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
    private const int increasedCritChance = 25;
    public override string GetDescription(CharacterPartials.Character character) =>
        $"Does a simple slash. Attack has an increased crit chance of {increasedCritChance}";
    

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this, usageType)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance + increasedCritChance,
            CriticalDamage = User.CriticalDamage,
            DamageDealer = User,
            Damage = User.Attack * 1.7f,
 
        });

        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            TargetType = TargetType.SingleTarget,
            Text = $"{User.NameWithAlphabet} does a simple slash to {target.NameWithAlphabet}!",
            User = User,
            UsageType = usageType
        };
    }
}
public class ConsecutiveSlashesOfPrecision : Ultimate
{
     const int increasedCritChance = 25;

     public override string GetDescription(CharacterPartials.Character character)
         => $"Slashes the enemy many times, dealing crazy damage. attack has an increased crit chance of "
            + $"{increasedCritChance}%";

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult =target.Damage(new DamageArgs(this, usageType)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance + increasedCritChance,
            CriticalDamage = User.CriticalDamage,
            DamageDealer = User,
            Damage = User.Attack * 1.7f *2,
            DamageText = $"The slash was so precise it dealt $ damage to {target.NameWithAlphabet}!",
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