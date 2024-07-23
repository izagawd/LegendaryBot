using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class BaseballBatWhack : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Swings a baseball bat at the enemy, causing solid  damage";
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
            Text = "Uraaah!",
            DamageResults =
            [
                target.Damage(new DamageArgs(this)
                {
                    ElementToDamageWith = User.Element,
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    Damage = User.Attack * 1.7f,
                    DamageDealer = User,
                    DamageText = $"{User.NameWithAlphabetIdentifier} whacks {target.NameWithAlphabetIdentifier} with a baseball bat, dealing $ damage"
                })
            ],
            User = User
        };
    }
}

public class Delinquent : CharacterPartials.Character
{

    public override Rarity Rarity => Rarity.TwoStar;

    public Delinquent()
    {
        TypeId = 10;
        BasicAttack = new BaseballBatWhack(){User = this};
    }
}