using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
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
                target.Damage(new DamageArgs
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
                    DamageText = $"{User.NameWithAlphabet} whacks {target.NameWithAlphabet} with a baseball bat, dealing $ damage"
                })
            ],
            User = User
        };
    }
}
public class DelinquentBeatdown : Skill
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Calls all other delinquents to beat up ally, dealing solid damage";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
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
                target.Damage(new DamageArgs
                {
                    DamageSource = new MoveDamageSource()
                    {
                        Move = this,
                        UsageType = usageType
                    },
                    ElementToDamageWith = User.Element,
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    Damage = User.Attack * 2.5f,
                    DamageDealer = User,
                    DamageText = $"many of {User.NameWithAlphabet}'s  friends beat up {target.NameWithAlphabet}, dealing $ damage"
                })
            ],
            User = User
        };
    }

    public override int MaxCooldown => 4;
}
public class Delinquent : CharacterPartials.Character
{
    public override Element Element => Element.Fire;



    public override Rarity Rarity => Rarity.TwoStar;

    public Delinquent()
    {
        TypeId = 10;
        BasicAttack = new BaseballBatWhack(){User = this};
        Skill = new DelinquentBeatdown();
    }
}