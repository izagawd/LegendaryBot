using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class GigaPunch : BasicAttack
{
    public override string GetDescription(Character character) => "Punch is thrown gigaly";
    
    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults =
            [
            target.Damage(new DamageArgs(this)
                {
                    ElementToDamageWith = User.Element,
                    CriticalDamage = User.CriticalDamage,
                    CriticalChance = User.CriticalChance,
                    Caster = User,
                    Damage = User.Attack * 1.7f,
                    DamageText = $"{User.NameWithAlphabetIdentifier} smiles chadly, and punches {target.NameWithAlphabetIdentifier} in a cool way and dealt $ damage!"

                })
            ],
            TargetType = TargetType.SingleTarget,
            User = User,
            UsageType = usageType,
            Text = "Hrrah!"
        };
    }
}

public class MuscleFlex : Ultimate
{
  
    public override string GetDescription(Character character) => "Flexes muscles";
    

    public override IEnumerable<Character> GetPossibleTargets()
    {
        yield return User;
    }

    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
    {
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabetIdentifier}... flexed his muscles?");
        return new UsageResult(this)
        {
            Text = $"Hmph!",
            TargetType = TargetType.None,
            User = User,
            UsageType = usageType
        };
    
    }

    public override int MaxCooldown => 1;
}

public class ThumbsUp : Skill
{

    public override string GetDescription(Character character) => "Gives the enemy a thumbs up!";
    

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
    {
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabetIdentifier} is cheering {target.NameWithAlphabetIdentifier} on!");
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            Text = $"{User.NameWithAlphabetIdentifier} gave {target.NameWithAlphabetIdentifier} a thumbs up!",
            User = User
        };

    }

    public override int MaxCooldown => 1;
}
public class CoachChad : Character, IBattleEventListener
{

    public override BasicAttack BasicAttack { get; } = new GigaPunch();

    public override Skill? Skill { get;  } = new ThumbsUp();
    public override Ultimate? Ultimate { get; } = new MuscleFlex();

    public override DiscordColor Color => DiscordColor.Purple;


    public override void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        List<BattleDecision> possibleDecisions = [BattleDecision.BasicAttack];
        
        
        if(Skill.CanBeUsed())
            possibleDecisions.Add(BattleDecision.Skill);
        if(Ultimate.CanBeUsed())
            possibleDecisions.Add(BattleDecision.Ultimate);
        decision = BasicFunctionality.RandomChoice(possibleDecisions.AsEnumerable());
        target = BasicFunctionality.RandomChoice(BasicAttack.GetPossibleTargets());


    }

    [BattleEventListenerMethod]
    private void HandleRevive(CharacterDeathEventArgs deathEventArgs)
    {
        
        if (deathEventArgs.Killed != this) return;
        Revive();
        
    }
    [BattleEventListenerMethod]
    private void HandleTurnEnd(TurnEndEventArgs turnEnd)
    {
        if (turnEnd.Character != this) return;
        RecoverHealth(100);

    }


}