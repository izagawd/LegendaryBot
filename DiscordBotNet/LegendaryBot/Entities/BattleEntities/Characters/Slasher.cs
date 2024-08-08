using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.Entities;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class WindSlash : Skill
{
    public override string Name => "Wind Slash";
    private const int increasedCritChance = 25;
    public override string GetDescription(Character character) => "Attacks all enemies with a sharp wind." +
        $" Attack has an increased crit chance of {increasedCritChance}%";
    

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
 
        foreach (var i in GetPossibleTargets())
        {
            i.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
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

    public WindSlash(Character user) : base(user)
    {
    }
}

public class SimpleSlashOfPrecision : BasicAttack
{
    public override string Name => "Simple Slash Of Precision";
    private const int increasedCritChance = 25;
    public override string GetDescription(Character character) =>
        $"Does a simple slash. Attack has an increased crit chance of {increasedCritChance}";
    

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f,
            new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance + increasedCritChance,
            CriticalDamage = User.CriticalDamage,
        });


        attackTargetType = AttackTargetType.SingleTarget;
            text = $"{User.NameWithAlphabet} does a simple slash to {target.NameWithAlphabet}!";

    }

    public SimpleSlashOfPrecision(Character user) : base(user)
    {
    }
}
public class ConsecutiveSlashesOfPrecision : Ultimate
{
    public override string Name => "Consecutive Slashes Of Precision";
     const int increasedCritChance = 25;

     public override string GetDescription(Character character)
         => $"Slashes the enemy many times, dealing crazy damage. attack has an increased crit chance of "
            + $"{increasedCritChance}%";

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        var damageResult =target.Damage(new DamageArgs( User.Attack * 1.7f *2,
            new MoveDamageSource(moveUsageContext))
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

    public ConsecutiveSlashesOfPrecision(Character user) : base(user)
    {
    }
}
public class Slasher : Character
{
    public override string Name => "Slasher";
    public override Rarity Rarity => Rarity.FourStar;
    public override BasicAttack GenerateBasicAttack()
    {
        return new SimpleSlashOfPrecision(this);
    }

    public override Skill? GenerateSkill()
    {
        return new WindSlash(this);
    }

    public override Ultimate? GenerateUltimate()
    {
        return new ConsecutiveSlashesOfPrecision(this);
    }

    public override DiscordColor Color => DiscordColor.Brown;

    public override Element Element => Element.Earth;

    public Slasher()
    {
        TypeId = 6;

       
    }

}