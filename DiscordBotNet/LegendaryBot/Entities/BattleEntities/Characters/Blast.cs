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
    protected override void UtilizeImplementation(Character target, UsageContext usageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
      
        target.Damage(new DamageArgs(User.Attack * 1.7f,new MoveDamageSource(usageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            
            CanCrit = true,
            DamageText = $"That was a harsh slap on {target.NameWithAlphabet} dealt $ damage!"
        });
        if (BasicFunctionality.RandomChance(DetonateChance))
        {
            foreach (var i in target.StatusEffects.OfType<Bomb>().ToArray())
                i.Detonate(User);
        }
        text =  "Methane Slap!";
        attackTargetType = AttackTargetType.SingleTarget;

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
    protected override void UtilizeImplementation(Character target, UsageContext usageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
                
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet} threw multiple bombs at the opposing team!");
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

        text = "Blow Away!";
        attackTargetType = AttackTargetType.AOE;

    }


}
public class ExplosionBlast : Ultimate
{
    public override string GetDescription(Character character) 
        => $"User does an explosion blast, attacking all enemies, inflicting burn x2 on each enemy hit";
    

    public override int MaxCooldown  => 5;
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }
    
    protected override void UtilizeImplementation(Character target, UsageContext usageContext,
        out AttackTargetType attackTargetType, out string? text)
    {
      
        foreach (var i in GetPossibleTargets())
        {
            i.Damage(new DamageArgs( User.Attack * 1.5f,new MoveDamageSource(usageContext))
            {
                ElementToDamageWith = User.Element,
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                DamageText =
                    $"{User.NameWithAlphabet} blasted {i.NameWithAlphabet}, dealing $ damage!",
            });
        }

        var eff = User.Effectiveness;
        foreach (var i in GetPossibleTargets())
        {
            i.AddStatusEffects([new Burn() { Caster = User, Duration = 1 },
                new Burn() { Caster = User, Duration = 1 }], eff);
        }

        text = "Blow Away!";
        attackTargetType = AttackTargetType.AOE;
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