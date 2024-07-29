using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class SoulAttack : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character) => "Uses the souls of the dead to attack, with a 25% chance to inflict sleep!";
    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType, 
        out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f,new MoveDamageSource(usageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} uses the souls of the dead to attack {target.NameWithAlphabet} and dealt $ damage!"
        });
        if (BasicFunctionality.RandomChance(25))
        {
            target.AddStatusEffect(new Sleep(){Caster = User, Duration = 1}, User.Effectiveness);
        }

        attackTargetType = AttackTargetType.SingleTarget;

        text = "Soul Attack!";

    }
}

public class YourLifeEnergyIsMine : Skill
{


    public override string GetDescription(CharacterPartials.Character character) => "Sucks the life energy out of the enemy, recovering 20% of damage dealt as hp";
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }
    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType, out string? text)
    {
        var damageResult = target.Damage(new DamageArgs(User.Attack * 2.5f,
            new MoveDamageSource(usageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} sucks the life essence out of {target.NameWithAlphabet} and deals $ damage!"
        });
   
        User.RecoverHealth(damageResult.DamageDealt * 0.2f);
        text = "Your lifespan is mine!";
        attackTargetType = AttackTargetType.SingleTarget;
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
    
    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out AttackTargetType attackTargetType, out string? text)
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
            i.AddStatusEffect(new Immortality(){Duration = duration, Caster = User}
            ,User.Effectiveness);
        }
        User.AddStatusEffect(new AttackBuff() { Duration = 3 , Caster = User},
            User.Effectiveness);
        User.GrantExtraTurn();
        text = "Necromancy!";
        attackTargetType = AttackTargetType.None;

    }
}
public class Thana : CharacterPartials.Character
{
    protected override float BaseSpeedMultiplier => 1.1f;
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