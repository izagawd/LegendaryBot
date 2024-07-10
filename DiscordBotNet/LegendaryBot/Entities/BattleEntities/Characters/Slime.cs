using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class GooeyStrike : BasicAttack
{
    
    public override string GetDescription(Character character) => "Slams it's body on the enemy, with a 10% chance to inflict poison";
    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Caster = User,
            Damage = User.Attack * 1.7f,
            DamageText = $"{User.NameWithAlphabetIdentifier} used a slime attack at {target.NameWithAlphabetIdentifier} and dealt $ damage!"
        });
        if (BasicFunctionality.RandomChance(10))
        {
            target.AddStatusEffect(new Poison(User));
        }
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
            Text = "Sulaimu attacku!",
            User = User
        };
    }
}
public class Slime : Character
{
    public override float GetSpeedValue(int points)
    {
        return (base.GetSpeedValue(points) * 0.8).Round();
    }

    public override float GetAttackValue(int points)
    {
        return (base.GetAttackValue(points) * 0.7).Round();
    }
    
    public override IEnumerable<Reward> DroppedRewards
    {
        get
        {
            if (BasicFunctionality.RandomChance(10))
                yield return new EntityReward([new Slime()]);
        }
    }

    public override Rarity Rarity { get; protected set; } = Rarity.TwoStar;

    public override BasicAttack BasicAttack { get; } = new GooeyStrike();
    public override Skill? Skill => null;
    public override Ultimate? Ultimate  => null;

}