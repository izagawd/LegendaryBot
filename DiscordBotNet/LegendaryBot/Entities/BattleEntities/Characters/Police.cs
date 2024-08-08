using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class DoNotResist : BasicAttack
{
    public override string Name => "Do Not Resist";
    public override string GetDescription(Character character)
    {
        return "Tases the enemy, with a 15% chance to stun for one turn";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
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

        attackTargetType = AttackTargetType.SingleTarget;


    }

    public DoNotResist(Character user) : base(user)
    {
    }
}

public class IAmShooting : Skill
{
    public override string Name => "I Am Shooting";
    public override string GetDescription(Character character)
    {
        return "Shoots the enemy twice, causing bleed for two turns";
    }
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 2,new MoveDamageSource(moveUsageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} shoots at {target.NameWithAlphabet} for resisting arrest, dealing $ damage"
        });
 
        target.AddStatusEffect(new Bleed(){ Caster = User, Duration = 2}, User.Effectiveness);
        text = "I warned you!";
        attackTargetType = AttackTargetType.SingleTarget;

    }

    public override int MaxCooldown => 3;

    public IAmShooting(Character user) : base(user)
    {
    }
}
public class Police : Character
{
    public override string Name => "Police";
    public override Rarity Rarity => Rarity.TwoStar;


    public override BasicAttack GenerateBasicAttack()
    {
        return new DoNotResist(this);
    }

    public override Skill? GenerateSkill()
    {
        return new IAmShooting(this);
    }

    public Police()
    {
        TypeId = 4;

     
    }


}