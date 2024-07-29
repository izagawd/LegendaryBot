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
    
    protected  override void UtilizeImplementation(Character target, UsageContext usageContext, out TargetType targetType, 
        out string? text)
    {
 
        var damageResult = target.Damage(       new DamageArgs(User.Attack * 1.7f,new MoveDamageSource(usageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,

            CanCrit = true,
            DamageText = $"{User.NameWithAlphabet} gave" +
                         $" {target.NameWithAlphabet} a punch and dealt $ damage!"
        });
        targetType = TargetType.SingleTarget;
        text = "very basic...";
    }
}