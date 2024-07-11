using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class SoulAttack : BasicAttack
{
    public override string GetDescription(Character character) => "Uses the souls of the dead to attack, with a 25% chance to sleep!";
    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            Caster = User,
            DamageText = $"{User.NameWithAlphabetIdentifier} uses the souls of the dead to attack {target.NameWithAlphabetIdentifier} and dealt $ damage!"
        });
        if (BasicFunctionality.RandomChance(25))
        {
            target.AddStatusEffect(new Sleep(User));
        }
        return new UsageResult(this)
        {
            TargetType = TargetType.SingleTarget,
            User = User,
            UsageType = usageType,
            DamageResults = [damageResult]
        };
    }
}

public class YourLifeEnergyIsMine : Skill
{


    public override string GetDescription(Character character) => "Sucks the life energy out of the enemy, recovering 20% of damage dealt as hp";
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }
    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 2.5f,
            Caster = User,
            DamageText = $"{User.NameWithAlphabetIdentifier} sucks the life essence out of {target.NameWithAlphabetIdentifier} and deals $ damage!"
        });
        if(damageResult is not null)
            User.RecoverHealth(damageResult.Damage * 0.2f);
        
        return new UsageResult(this)
        {
            DamageResults =
            [
            
                damageResult
            ],
            Text = "Your lifespan is mine!",
            User = User,
            TargetType = TargetType.SingleTarget,
            UsageType = usageType
        };
    }

    public override int MaxCooldown => 3;
}
public class Arise : Ultimate
{

    public override int MaxCooldown =>6;

    public override string GetDescription(Character character) =>
        $"Grants all allies immortality, increases the caster's attack for 2 turns, and grants her an extra turn";
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.Team;
    }
    
    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
    {
        User.CurrentBattle.AddAdditionalBattleText($"With her necromancy powers, {User.NameWithAlphabetIdentifier} attempts to bring back all her dead allies!");

        foreach (var i in GetPossibleTargets())
        {
            if(i.IsDead)
                i.Revive();

            var duration = 1;
            if (i == User)
            {
                duration = 3;
            }
            i.AddStatusEffect(new Immortality(User){Duration = duration});
        }
        User.AddStatusEffect(new AttackBuff(User) { Duration = 3 });
        User.GrantExtraTurn();
        return new UsageResult(this)
        {
            User = User,
            TargetType = TargetType.AOE,
            Text = "Necromancy!",
            UsageType = usageType
        };
    }
}
public class Thana : Character
{

    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;
    public override DiscordColor Color { get; protected set; } = DiscordColor.Brown;

    public override Element Element => Element.Earth;
    public override BasicAttack BasicAttack { get; } = new SoulAttack();

    public override Skill? Skill { get; } = new YourLifeEnergyIsMine();
    public override Ultimate? Ultimate { get; } = new Arise();
}