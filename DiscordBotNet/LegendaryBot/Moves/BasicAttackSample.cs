using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;
/// <summary>
/// A sample class for basic attack
/// </summary>
public class BasicAttackSample : BasicAttack
{
    public override string GetDescription(Character character) => "Take that!";
    
    protected  override UsageResult UtilizeImplementation(Character target, UsageType usageType)
    {
 
        var damageResult = target.Damage(       new DamageArgs
        {
            DamageSource = new MoveDamageSource()
            {
                Move = this,
                UsageType = usageType
            },
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            DamageDealer = User,
            CanCrit = true,
            DamageText = $"{User.NameWithAlphabet} gave" +
                         $" {target.NameWithAlphabet} a punch and dealt $ damage!"
        });
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = User,
            DamageResults = [damageResult]
        };
    }
}