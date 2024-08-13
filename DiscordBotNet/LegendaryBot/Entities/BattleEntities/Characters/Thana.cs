using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class SoulAttack : BasicAttack
{
    public override string Name => "Soul Attack";
    public override string GetDescription(Character character) => "Uses the souls of the dead to attack, with a 25% chance to inflict sleep!";
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, 
        out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f,new MoveDamageSource(moveUsageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            DamageText = $"{User.NameWithAlphabet} uses the souls of the dead to attack {target.NameWithAlphabet} and dealt $ damage!"
        });
        if (BasicFunctionality.RandomChance(25))
        {
            target.AddStatusEffect(new Sleep(User){ Duration = 1}, User.Effectiveness);
        }

        attackTargetType = AttackTargetType.SingleTarget;

        text = "Soul Attack!";

    }

    public SoulAttack(Character user) : base(user)
    {
    }
}

public class YourLifeEnergyIsMine : Skill
{
    public override string Name => "Your Life Energy Is Mine";

    public override string GetDescription(Character character) => "Sucks the life energy out of the enemy, recovering 20% of damage dealt as hp";
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        var damageResult = target.Damage(new DamageArgs(User.Attack * 2.5f,
            new MoveDamageSource(moveUsageContext))
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

    public YourLifeEnergyIsMine(Character user) : base(user)
    {
    }
}
public class Arise : Ultimate
{
    public override string Name => "Arise!!";
    public override int MaxCooldown =>6;

    public override string GetDescription(Character character) =>
        $"Revives dead allies, grants all allies immortality, increases the caster's attack for 2 turns, and grants her an extra turn";
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.Team;
    }
    
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        User.CurrentBattle.AddBattleText($"With her necromancy powers, {User.NameWithAlphabet} attempts to bring back all her dead allies!");

     


        foreach (var i in GetPossibleTargets())
        {
            if(i.IsDead)
                i.Revive();
        }
        foreach (var i in GetPossibleTargets().OrderBy(i => i == User ? 1 : 0))
        {
            var duration = 1;
            if (i == User)
            {
                duration = 3;
            }
            i.AddStatusEffect(new Immortality(User){Duration = duration}
            ,User.Effectiveness);
        }
        User.AddStatusEffect(new AttackBuff(User) { Duration = 3 },
            User.Effectiveness);
        User.GrantExtraTurn();
        text = "Necromancy!";
        attackTargetType = AttackTargetType.None;

    }

    public Arise(Character user) : base(user)
    {
    }
}
public class Thana : Character
{
    public override string Name => "Thana";
    protected override float BaseSpeedMultiplier => 1.1f;
    public override Rarity Rarity =>Rarity.FiveStar;
    public override DiscordColor Color => DiscordColor.Brown;

    public override Element Element => Element.Earth;


    public override int TypeId
    {
        get => 8;
        protected init {}
    }


    public Thana()
    {

        Ultimate = new Arise(this);
        BasicAttack = new SoulAttack(this);
        Skill = new YourLifeEnergyIsMine(this);

    }

}