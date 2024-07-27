using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class SlimeBodySlam : BasicAttack
{
    
    public override string GetDescription(CharacterPartials.Character character) => "Slams it's body on the enemy, with a 10% chance to inflict poison";
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs
        {
            DamageSource = new MoveDamageSource()
            {
                Move = this,
                UsageType = usageType
            },
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageDealer = User,
            Damage = User.Attack * 1.7f,
            DamageText = $"{User.NameWithAlphabet} slams at {target.NameWithAlphabet} and dealt $ damage!"
        });
        if (BasicFunctionality.RandomChance(10))
        {
            target.AddStatusEffect(new Poison(){Caster = User, Duration = 1});
        }
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
            Text = "Slime body slam!",
            User = User
        };
    }
}
public class Slime : CharacterPartials.Character
{

    protected override float BaseSpeedMultiplier => 0.8f;
    protected override float BaseMaxHealthMultiplier => 0.7f;
    protected override float BaseAttackMultiplier => 0.4f;


    public override Rarity Rarity => Rarity.OneStar;


    public Slime()
    {
        TypeId = 7;
        BasicAttack = new SlimeBodySlam(){User = this};
        
    }


}