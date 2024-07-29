using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class DoNotResist : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Tases the enemy, with a 15% chance to stun for one turn";
    }

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out TargetType targetType, out string? text)
    {
        var damageResult= target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(usageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} tases {target.NameWithAlphabet} and dealt $ damage! it was shocking"
        });
        if (BasicFunctionality.RandomChance(15))
        {
            target.AddStatusEffect(new Stun(){Duration = 1, Caster = User}, User.Effectiveness);
        }


        text = "DO NOT RESIST!";

        targetType = TargetType.SingleTarget;


    }
}

public class IAmShooting : Skill
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Shoots the enemy twice, causing bleed for two turns";
    }
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, UsageContext usageContext, out TargetType targetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 2,new MoveDamageSource(usageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} shoots at {target.NameWithAlphabet} for resisting arrest, dealing $ damage"
        });
 
        target.AddStatusEffect(new Bleed(){ Caster = User, Duration = 2}, User.Effectiveness);
        text = "I warned you!";
        targetType = TargetType.SingleTarget;

    }

    public override int MaxCooldown => 3;
}
public class Police : CharacterPartials.Character
{

    public override Rarity Rarity => Rarity.TwoStar;



    public Police()
    {
        TypeId = 4;
        BasicAttack = new DoNotResist(){User = this};
        Skill = new IAmShooting(){User = this};
     
    }


}