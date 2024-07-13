using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class DoNotResist : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Tases the enemy, with a 15% chance to stun for one turn";
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult= target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            Caster = User,
            DamageText = $"{User.NameWithAlphabetIdentifier} tases {target.NameWithAlphabetIdentifier} and dealt $ damage! it was shocking"
        });
        if (BasicFunctionality.RandomChance(15))
        {
            target.AddStatusEffect(new Stun(User){Duration = 1}, User.Effectiveness);
        }

        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            Text = "DO NOT RESIST!",
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = User,

        };
    }
}

public class IAmShooting : Skill
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Shoots the enemy twice, causing two bleed effects for two turns";
    }
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Caster = User,
            Damage = User.Attack * 2,
            DamageText = $"{User.NameWithAlphabetIdentifier} shoots at {target.NameWithAlphabetIdentifier} for resisting arrest, dealing $ damage"
        });
        foreach (var _ in Enumerable.Range(0,2))
        {
            target.AddStatusEffect(new Bleed(User), User.Effectiveness);
        }
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            Text = "I warned you!",
            User = User,
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
        };
    }

    public override int MaxCooldown => 3;
}
public class Police : CharacterPartials.Character
{
    public override Rarity Rarity => Rarity.TwoStar;

    public override IEnumerable<Reward> DroppedRewards
    {
        get
        {
            if (BasicFunctionality.RandomChance(20))
            {
                yield return new EntityReward([new Police()]);
            }

        }
    }

    public Police()
    {
        BasicAttack = new DoNotResist(){User = this};
        Skill = new IAmShooting(){User = this};
     
    }


}