using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;

/// <summary>
///     A sample class for basic attack
/// </summary>
public class BasicAttackSample : BasicAttack
{
    public BasicAttackSample(Character user) : base(user)
    {
    }

    public override string Name => "Basic Attack Sample";

    public override string GetDescription(Character character)
    {
        return "Take that!";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
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