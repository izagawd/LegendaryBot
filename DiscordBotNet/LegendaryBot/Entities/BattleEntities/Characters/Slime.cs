using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class SlimeBodySlam : BasicAttack
{
    public override string Name => "Slime Body Slam";
    public override string GetDescription(Character character) => "Slams it's body on the enemy, with a 10% chance to inflict poison";
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f,new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} slams at {target.NameWithAlphabet} and dealt $ damage!",
        });
        if (BasicFunctionality.RandomChance(10))
        {
            target.AddStatusEffect(new Poison(){Caster = User, Duration = 1}, User.Effectiveness);
        }
        attackTargetType = AttackTargetType.SingleTarget;

        text = "Slime body slam!";
        
    }
}
public class Slime : Character
{
    public override string Name => "Slime";
    protected override float BaseSpeedMultiplier => 0.8f;
    protected override float BaseMaxHealthMultiplier => 0.7f;
    protected override float BaseAttackMultiplier => 0.4f;


    public override Rarity Rarity => Rarity.TwoStar;


    public Slime()
    {
        TypeId = 7;
        BasicAttack = new SlimeBodySlam(){User = this};
        
    }


}