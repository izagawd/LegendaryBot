using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class ChamomileSachetWhack : BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character) => 
        $"With the power of Chamomile, whacks an enemy with a sack filled with Chamomile, with a {SleepChance}% chance of making the enemy sleep";
    

    public int SleepChance => 25;
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this, usageType)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            DamageDealer = User,
            CanCrit = true,
            DamageText = $"That was a harsh snoozy whack that dealt $ damage on {target.NameWithAlphabet}!",

        });
        var result = new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = User,
            Text = "Chamomile Whack!",
            DamageResults = [damageResult],
        };
    

        if (BasicFunctionality.RandomChance(SleepChance))
        {
            target.AddStatusEffect(new Sleep(){Caster = User, Duration = 1}, User.Effectiveness);
        }
        return result;
    }
}
public class BlossomTouch : Skill
{
    public override int MaxCooldown => 3;

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.Team.Where(i =>!i.IsDead);
    }

    public int HealthHealScaling => 30;
  
    public override string GetDescription(CharacterPartials.Character character) =>  $"With the power of flowers, recovers the hp of an ally with {HealthHealScaling}% of the caster's max health, dispelling one debuff";
    
 
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        target.RecoverHealth((User.MaxHealth *HealthHealScaling* 0.01).Round());
        return new UsageResult(this)
        {
            Text = $"{User.NameWithAlphabet} used Blossom Touch!",
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = User
        };
    }
}
public class LilyOfTheValley : Ultimate
{
    public override int MaxCooldown  => 5;

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

 
    public override  string GetDescription(CharacterPartials.Character character) => $"Releases a poisonous gas to a single enemy,  inflicting stun for 1 turn, and inflicts poison x2 for 2 turns";
    

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
           
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabet} used Lily of The Valley, and released a dangerous gas to {target.NameWithAlphabet}!");
      
        var effectiveness = User.Effectiveness;

        target.AddStatusEffects([new Poison(){Duration = 2,Caster = User},
        new Poison(){Duration = 2, Caster = User},new Stun(){Duration = 1, Caster = User}],effectiveness);
  
        return new UsageResult(this)
        {
            Text =  $"The valley!",
            TargetType = TargetType.AOE,
            UsageType = usageType,
            User = User
        };
    }
}
public class Lily : CharacterPartials.Character
{
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

    public override void NonPlayerCharacterAi(ref CharacterPartials.Character target, ref BattleDecision decision)
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


    public Lily()
    {
        TypeId = 3;
        Skill = new BlossomTouch(){User = this};
        Ultimate  = new LilyOfTheValley(){User = this};
        BasicAttack = new ChamomileSachetWhack(){User = this};
        
    }

    public override Element Element => Element.Earth;


}
