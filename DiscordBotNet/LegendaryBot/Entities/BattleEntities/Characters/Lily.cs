using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class ChamomileSachetWhack : BasicAttack
{
    public override string Name => "Chamomile Sachet Whack";
    public override string GetDescription(Character character) => 
        $"With the power of Chamomile, whacks an enemy with a sack filled with Chamomile, with a {SleepChance}% chance of making the enemy sleep";
    

    public int SleepChance => 25;
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, 
        out AttackTargetType attackTargetType, out string? text)
    {
        target.Damage(new DamageArgs(User.Attack * 1.7f,new MoveDamageSource(moveUsageContext))
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            CanCrit = true,
            DamageText = $"That was a harsh snoozy whack that dealt $ damage on {target.NameWithAlphabet}!",
        });
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Chamomile Whack!";
        if (BasicFunctionality.RandomChance(SleepChance))
        {
            target.AddStatusEffect(new Sleep(){Caster = User, Duration = 1}, User.Effectiveness);
        }

    }

    public ChamomileSachetWhack(Character user) : base(user)
    {
    }
}
public class BlossomTouch : Skill
{
    public override string Name => "Blossom Touch";
    public override int MaxCooldown => 3;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.Team.Where(i =>!i.IsDead);
    }

    public int HealthHealScaling => 30;
  
    public override string GetDescription(Character character) =>  $"With the power of flowers, recovers the hp of an ally with {HealthHealScaling}% of the caster's max health, dispelling one debuff";
    
 
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        target.RecoverHealth((User.MaxHealth *HealthHealScaling* 0.01).Round());

        text = $"{User.NameWithAlphabet} used Blossom Touch!";
        attackTargetType = AttackTargetType.None;

    }

    public BlossomTouch(Character user) : base(user)
    {
    }
}
public class LilyOfTheValley : Ultimate
{
    public override string Name => "Lily Of The Valley";
    public override int MaxCooldown  => 5;

    public override IEnumerable<Character> GetPossibleTargets()
    {
        
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

 
    public override  string GetDescription(Character character) => $"Releases a poisonous gas to a single enemy,  inflicting stun for 1 turn, and inflicts poison x2 for 2 turns";
    

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
           
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet} used Lily of The Valley, and released a dangerous gas to {target.NameWithAlphabet}!");
      
        var effectiveness = User.Effectiveness;

        target.AddStatusEffects([new Poison(){Duration = 2,Caster = User},
        new Poison(){Duration = 2, Caster = User},new Stun(){Duration = 1, Caster = User}],effectiveness);
        text = $"The valley!";
        attackTargetType = AttackTargetType.None;

    }

    public LilyOfTheValley(Character user) : base(user)
    {
    }
}
public class Lily : Character
{
    public override string Name => "Lily";
    protected override float BaseSpeedMultiplier => 1.15f;


 
    public override Rarity Rarity => Rarity.FourStar;


    public override DiscordColor Color => DiscordColor.HotPink;
    
    protected override IEnumerable<StatType> LevelMilestoneStatIncrease =>
    [
        StatType.MaxHealth, StatType.Defense,
        StatType.Effectiveness, StatType.Effectiveness, StatType.MaxHealth, StatType.Defense
    ];
    protected override float BaseMaxHealthMultiplier => 1.1f;
    protected override float BaseDefenseMultiplier => 1.05f;

    protected override float BaseAttackMultiplier => 0.9f;
    public override bool CanSpawnNormally => false;

    public override void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        if (Ultimate.CanBeUsed())
        {
            decision = BattleDecision.Ultimate;
            target = BasicFunctionality.RandomChoice(Ultimate.GetPossibleTargets());
            return;
        }

        var teamMateWithLowestHealth = Team.OrderBy(i => i.Health).First();
        if (Skill.CanBeUsed() && teamMateWithLowestHealth.Health < teamMateWithLowestHealth.MaxHealth * 0.7)
        {
            decision = BattleDecision.Skill;
            target = teamMateWithLowestHealth;
            return;
        }

        decision = BattleDecision.BasicAttack;
        
        target = BasicAttack.GetPossibleTargets().OrderBy(i => i.Health).First();

    }

    public override Skill? GenerateSkill()
    {
        return new BlossomTouch(this);
    }

    public override Ultimate? GenerateUltimate()
    {
        return new LilyOfTheValley(this);
    }

    public override BasicAttack GenerateBasicAttack()
    {
        return new ChamomileSachetWhack(this);
    }
    public Lily()
    {
        TypeId = 3;

    }

    public override Element Element => Element.Earth;


}
