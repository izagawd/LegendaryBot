using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class SoulAttack : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character) => "Uses the souls of the dead to attack, with a 25% chance to inflict sleep!";
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs
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
            DamageText = $"{User.NameWithAlphabet} uses the souls of the dead to attack {target.NameWithAlphabet} and dealt $ damage!"
        });
        if (BasicFunctionality.RandomChance(25))
        {
            target.AddStatusEffect(new Sleep(){Caster = User, Duration = 1});
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


    public override string GetDescription(CharacterPartials.Character character) => "Sucks the life energy out of the enemy, recovering 20% of damage dealt as hp";
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs
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
            DamageText = $"{User.NameWithAlphabet} sucks the life essence out of {target.NameWithAlphabet} and deals $ damage!"
        });
   
        User.RecoverHealth(damageResult.DamageDealt * 0.2f);
        
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

    public override string GetDescription(CharacterPartials.Character character) =>
        $"Revives dead allies, grants all allies immortality, increases the caster's attack for 2 turns, and grants her an extra turn";
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.Team;
    }
    
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        User.CurrentBattle.AddBattleText($"With her necromancy powers, {User.NameWithAlphabet} attempts to bring back all her dead allies!");

        var possibleTargets = GetPossibleTargets().ToArray();


        foreach (var i in possibleTargets)
        {
            if(i.IsDead)
                i.Revive();
        }
        foreach (var i in possibleTargets.OrderBy(i => i == User ? 1 : 0))
        {
            var duration = 1;
            if (i == User)
            {
                duration = 3;
            }
            i.AddStatusEffect(new Immortality(){Duration = duration, Caster = User});
        }
        User.AddStatusEffect(new AttackBuff() { Duration = 3 , Caster = User});
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
public class Thana : CharacterPartials.Character
{

    public override Rarity Rarity =>Rarity.FiveStar;
    public override DiscordColor Color => DiscordColor.Brown;

    public override Element Element => Element.Earth;


    public Thana()
    {
        TypeId = 8;
        BasicAttack = new SoulAttack(){User = this};
        Skill = new YourLifeEnergyIsMine(){User = this};
        Ultimate = new Arise(){User = this};
        
    }

}