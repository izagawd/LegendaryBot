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
    
    protected  override UsageResult HiddenUtilize(Character target, UsageType usageType)
    {
 
        DamageResult? damageResult = target.Damage(       new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            Caster = User,
            CanCrit = true,
            DamageText = $"{User.NameWithAlphabetIdentifier} gave" +
                         $" {target.NameWithAlphabetIdentifier} a punch and dealt $ damage!"
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