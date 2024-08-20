using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Results;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Moves;

/// <summary>
///     A sample class for basic attack
/// </summary>
public class BasicAttackSample : BasicAttack
{
    public BasicAttackSample(CharacterPartials_Character user) : base(user)
    {
    }

    public override string Name => "Basic Attack Sample";

    public override string GetDescription(CharacterPartials_Character character)
    {
        return "Take that!";
    }

    protected override void UtilizeImplementation(CharacterPartials_Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType,
        out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,

            CanCrit = true,
            DamageText = $"{User.NameWithAlphabet} gave" +
                         $" {target.NameWithAlphabet} a punch and dealt $ damage!"
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = "very basic...";
    }
}