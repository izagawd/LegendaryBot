using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class ThugPunch : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Punches the enemy in a thug way";
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
                    Damage = User.Attack * 1.5f,
                    DamageDealer = User,
                    DamageText = $"{User.NameWithAlphabetIdentifier} punches {target.NameWithAlphabetIdentifier} with terrible battle stance, dealing $ damage!"
                })
            ],
            User = User
        };
    }
}

public class Thug : CharacterPartials.Character
{

    public Thug()
    {
        TypeId = 9;
        BasicAttack = new ThugPunch(){User = this};
        
    }

}