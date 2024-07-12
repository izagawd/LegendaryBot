using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class MethaneSlap : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character) => $"Slaps the enemy, " +
                                                                  $"producing methane around the enemy, with a " +
                                                                  $"{DetonateChance}% chance to detonate all the bombs the target has";
    public int DetonateChance => 75;
    protected override UsageResult HiddenUtilize(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            Caster = User,
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
            foreach (var i in target.StatusEffects.OfType<Bomb>())
                i.Detonate(User);
        }
        return result;

    }
}
public class BlowAway : Skill
{
    
    public override int MaxCooldown => 4;
    public override string GetDescription(CharacterPartials.Character character) => $"Throws multiple bombs at the enemy, with a {BombInflictChance} each to inflict Bomb status effect";

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    public int BombInflictChance => 100;
    protected override UsageResult HiddenUtilize(CharacterPartials.Character target, UsageType usageType)
    {
                
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabetIdentifier} threw multiple bombs at the opposing team!");
        foreach (var i in GetPossibleTargets())
        {

            foreach (var _ in Enumerable.Range(0,1))
            {
                if (BasicFunctionality.RandomChance(BombInflictChance))
                {
                                
                    i.AddStatusEffect(new Bomb(User){Duration = 2}, User.Effectiveness);
                }
            }

        }

        return new UsageResult(this){TargetType = TargetType.AOE,Text = "Blow Away!",User = User,UsageType = usageType};
        
    }


}
public class VolcanicEruption : Ultimate
{
    public override string GetDescription(CharacterPartials.Character character) => $"Makes the user charge up a very powerful explosion that hits all enemies for 4 turns!";
    

    public override int MaxCooldown  => 6;
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(CharacterPartials.Character target, UsageType usageType)
    {
        var isCharging = User.AddStatusEffect(new VolcanicEruptionCharging(User){Duration = 3});
        if(isCharging == StatusEffectInflictResult.Succeeded)
            User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabetIdentifier} is charging up a very powerful attack!");
        return new UsageResult(this){UsageType = usageType, TargetType = TargetType.AOE, User = User, Text = "What's this?"};
    }
}
public class Blast : CharacterPartials.Character
{
    public override Rarity Rarity { get; protected set; } = Rarity.FourStar;
    public override DiscordColor Color { get; protected set; } = DiscordColor.Brown;



    public override Ultimate? Ultimate { get;  } = new VolcanicEruption();
    public override Skill? Skill { get; } = new BlowAway();
    public override BasicAttack BasicAttack { get; } = new MethaneSlap();
}