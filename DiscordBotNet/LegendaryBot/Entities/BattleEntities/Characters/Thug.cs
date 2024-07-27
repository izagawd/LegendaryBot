using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

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
                    Damage = User.Attack * 1.5f,
                    DamageDealer = User,
                    DamageText = $"{User.NameWithAlphabet} punches {target.NameWithAlphabet} with terrible battle stance, dealing $ damage!"
                })
            ],
            User = User
        };
    }
}
public class ThugInsult : Skill
{
    public override string GetDescription(CharacterPartials.Character character)
    {
        return "Insults the enemy, decreasing defense for 2 turns";
    }

    public override IEnumerable<Character> GetPossibleTargets()
    {
       return CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabet} insults {target.NameWithAlphabet} like a thug!");
        target.AddStatusEffect(new DefenseDebuff() { Caster = User, Duration = 2 }, User.Effectiveness);
        return new UsageResult(this)
        {
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
            Text = "What you gonna do about it?",
            User = User
        };
    }

    public override int MaxCooldown => 4;
}
public class Thug : CharacterPartials.Character
{

    public Thug()
    {
        TypeId = 9;
        BasicAttack = new ThugPunch(){User = this};
        Skill = new ThugInsult();

    }

}