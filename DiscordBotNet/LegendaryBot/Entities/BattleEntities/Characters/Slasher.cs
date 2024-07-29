using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.Entities;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class WindSlash : Skill
{
    private const int increasedCritChance = 25;
    public override string GetDescription(CharacterPartials.Character character) => "Attacks all enemies with a sharp wind." +
        $" Attack has an increased crit chance of {increasedCritChance}%";
    

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType, out string? text)
    {
 
        foreach (var i in GetPossibleTargets())
        {
            i.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(usageContext))
            {

                ElementToDamageWith = User.Element,
                CriticalChance = User.CriticalChance + increasedCritChance,
                CriticalDamage = User.CriticalDamage,

                DamageText = $"The slash dealt $ damage to {i}!"
            });
        }


        attackTargetType = AttackTargetType.AOE;

            text = "Wind Slash!";

    }

    public override int MaxCooldown => 2;
}

public class SimpleSlashOfPrecision : BasicAttack
{
    private const int increasedCritChance = 25;
    public override string GetDescription(CharacterPartials.Character character) =>
        $"Does a simple slash. Attack has an increased crit chance of {increasedCritChance}";
    

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f,
            new MoveDamageSource(usageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance + increasedCritChance,
            CriticalDamage = User.CriticalDamage,
        });


        attackTargetType = AttackTargetType.SingleTarget;
            text = $"{User.NameWithAlphabet} does a simple slash to {target.NameWithAlphabet}!";

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

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType, out string? text)
    {
        var damageResult =target.Damage(new DamageArgs( User.Attack * 1.7f *2,
            new MoveDamageSource(usageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance + increasedCritChance,
            CriticalDamage = User.CriticalDamage,
      
            DamageText = $"The slash was so precise it dealt $ damage to {target.NameWithAlphabet}!",
        });

        attackTargetType = AttackTargetType.SingleTarget;
        text = null;
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