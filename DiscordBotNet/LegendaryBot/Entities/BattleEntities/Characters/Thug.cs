using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class ThugPunch : BasicAttack
{
    public override string Name => "Thug Punch";
    public override string GetDescription(Character character)
    {
        return "Punches the enemy in a thug way";
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.5f, new MoveDamageSource(moveUsageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText =
                $"{User.NameWithAlphabet} punches {target.NameWithAlphabet} with terrible battle stance, dealing $ damage!"
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Uraah!";
    }

    public ThugPunch(Character user) : base(user)
    {
    }
}
public class ThugInsult : Skill
{
    public override string Name => "Thug Insult";
    public override string GetDescription(Character character)
    {
        return "Insults the enemy, decreasing defense for 2 turns";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
       return CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
        out string? text)
    {
        CurrentBattle.AddBattleText($"{User.NameWithAlphabet} insults {target.NameWithAlphabet} like a thug!");
        target.AddStatusEffect(new DefenseDebuff(User) {  Duration = 2 }, User.Effectiveness);
        attackTargetType = AttackTargetType.SingleTarget;
        text = "What are you gonna do about it?";
    }

    public override int MaxCooldown => 4;

    public ThugInsult(Character user) : base(user)
    {
    }
}
public class Thug : Character
{
    public override string Name => "Thug";

    public override BasicAttack GenerateBasicAttack()
    {
        return new ThugPunch(this);
    }

    public override Skill? GenerateSkill()
    {
        return new ThugInsult(this);
    }

    public Thug()
    {
        TypeId = 9;


    }

}