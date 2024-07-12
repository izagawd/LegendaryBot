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
    

    public int SleepChance => 40;
    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 1.7f,
            Caster = User,
            CanCrit = true,
            DamageText = $"That was a harsh snoozy whack that dealt $ damage on {target.NameWithAlphabetIdentifier}!",

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
            target.AddStatusEffect(new Sleep(User), User.Effectiveness);
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
            Text = $"{User.NameWithAlphabetIdentifier} used Blossom Touch!",
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

    public int PoisonInflictChance => 100;

    public int StunInflictChance => 50;
    public override  string GetDescription(CharacterPartials.Character character) => $"Releases a poisonous gas to all enemies, with an {StunInflictChance}% chance of inflicting stun for 1 turn and a {PoisonInflictChance}% chance of inflicting poison for one turn";
    

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        List<StatusEffect> statusEffects = [];
        var effectiveness = User.Effectiveness;
        foreach (var i in GetPossibleTargets())
        {
            
            if (BasicFunctionality.RandomChance(PoisonInflictChance))
            {
                statusEffects.Add(new Poison(User){Duration = 2});
            }
            if (BasicFunctionality.RandomChance(StunInflictChance))
            {
                statusEffects.Add(new Stun(User){Duration = 2});
            }
            if(statusEffects.Any()) i.AddStatusEffects(statusEffects,effectiveness);
            statusEffects.Clear();
        }

        return new UsageResult(this)
        {
            Text =  $"{User.NameWithAlphabetIdentifier} used Lily of The Valley, and released a dangerous gas to the enemy team!",
            TargetType = TargetType.AOE,
            UsageType = usageType,
            User = User
        };
    }
}
public class Lily : CharacterPartials.Character
{
    protected override float BaseSpeedMultiplier => 1.15f;


    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;
    public override DiscordColor Color { get; protected set; } = DiscordColor.HotPink;

    public override void NonPlayerCharacterAi(ref CharacterPartials.Character target, ref BattleDecision decision)
    {
        if (Ultimate.CanBeUsed())
        {
            decision = BattleDecision.Ultimate;
            target = Ultimate.GetPossibleTargets().First();
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
        Skill = new BlossomTouch(){User = this};
        Ultimate  = new LilyOfTheValley(){User = this};
        BasicAttack = new ChamomileSachetWhack(){User = this};
        
    }

    public override Element Element => Element.Earth;


}
