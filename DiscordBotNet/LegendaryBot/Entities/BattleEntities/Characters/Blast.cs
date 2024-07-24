using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class MethaneSlap : BasicAttack
{
    public override string GetDescription(Character character) => $"Slaps the enemy, " +
                                                                  $"producing methane around the enemy, with a " +
                                                                  $"{DetonateChance}% chance to detonate all the bombs the target has";
    public int DetonateChance => 75;
    protected override UsageResult UtilizeImplementation(Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            DamageDealer = User,
            CanCrit = true,
            DamageText = $"That was a harsh slap on {target.NameWithAlphabetIdentifier} dealt $ damage!"
        });
        var damageResultList = new []{ damageResult };
        var result = new UsageResult(this)
        {
            Text = "Methane Slap!",User = User,
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            DamageResults = damageResultList
        };
        if (BasicFunctionality.RandomChance(DetonateChance))
        {
            foreach (var i in target.StatusEffects.OfType<Bomb>().ToArray())
                i.Detonate(User);
        }
        return result;

    }
}
public class BlowAway : Skill
{
    
    public override int MaxCooldown => 4;
    public override string GetDescription(Character character) => $"Throws multiple bombs at the enemy, with a {BombInflictChance}% chance each to inflict Bomb status effect";

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    public int BombInflictChance => 100;
    protected override UsageResult UtilizeImplementation(Character target, UsageType usageType)
    {
                
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabetIdentifier} threw multiple bombs at the opposing team!");
        foreach (var i in GetPossibleTargets())
        {

            foreach (var _ in Enumerable.Range(0,1))
            {
                if (BasicFunctionality.RandomChance(BombInflictChance))
                {
                                
                    i.AddStatusEffect(new Bomb(){Duration = 2, Caster = User}, User.Effectiveness);
                }
            }

        }

        return new UsageResult(this){TargetType = TargetType.AOE,Text = "Blow Away!",User = User,UsageType = usageType};
        
    }


}
public class ExplosionBlast : Ultimate
{
    public override string GetDescription(Character character) => $"User does an explosion blast, attacking all enemies!";
    

    public override int MaxCooldown  => 6;
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(Character target, UsageType usageType)
    {
        
        foreach (var i in GetPossibleTargets())
        {
            
        }
        return new UsageResult(this){UsageType = usageType, TargetType = TargetType.AOE, User = User, Text = "What's this?"};
    }
}
public class Blast : Character
{

    public override Rarity Rarity => Rarity.FourStar;
    public override DiscordColor Color => DiscordColor.Brown;

    protected override float BaseSpeedMultiplier => 1.1f;
    protected override float BaseAttackMultiplier => 1.05f;




    public Blast()
    {
        TypeId = 2;
        Ultimate = new ExplosionBlast(){User = this};
        Skill = new BlowAway(){User = this};
        BasicAttack = new MethaneSlap(){User = this};
      
    }
    


}