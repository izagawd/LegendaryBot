using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class TakeshiStraightPunch : BasicAttack
{
    public override string GetDescription(Character character)
    {
        return "Does a simple but powerful straight punch at the enemy!";
    }

    protected override UsageResult UtilizeImplementation(Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults =
            [
                target.Damage(new DamageArgs(this, usageType)
                {
                    DamageText =
                        $"{User.NameWithAlphabet} does a straight punch at {target.NameWithAlphabet}, dealing $ damage!",
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    ElementToDamageWith = User.Element,
                    Damage = User.Attack * 1.7f,
                    DamageDealer = User
                })
            ],
            TargetType = TargetType.SingleTarget,
            User = User,
            UsageType = usageType
        };
    }
}
public class Takeshi : Character, IBattleEventListener
{
    public override Rarity Rarity => Rarity.ThreeStar;
    public override string? PassiveDescription => "Has a 50% chance to counter attack with basic attack when attacked";

    [NotMapped]
    private Character _targetCharacter = null;
    [BattleEventListenerMethod]
    public void OnHitByMove(CharacterPostDamageEventArgs args)
    {
        var damageResult = args.DamageResult;
        if(!damageResult.MoveUsageDetails.HasValue)
            return;
        var move = damageResult.MoveUsageDetails.Value.Move;
        if (args.DamageResult.MoveUsageDetails!.Value.UsageType == UsageType.CounterUsage)
            return;
        if(move is null) return;
        if(CannotDoAnything) return;
        if(damageResult.DamageReceiver != this) return;
        if(damageResult.DamageDealer is null) return;
        if (BasicFunctionality.RandomChance(50))
        {
            _targetCharacter = damageResult.DamageDealer;
        }
    }
    [BattleEventListenerMethod]
    public void CounterAttackUsageRefresh(CharacterPostUseMoveEventArgs args)
    {
        if (!CannotDoAnything && _targetCharacter is not null  && !_targetCharacter.IsDead)
        {
            BasicAttack.Utilize(_targetCharacter, UsageType.CounterUsage);
            _targetCharacter = null;
        }
    }
    public Takeshi()
    {
        TypeId = 13;
        BasicAttack = new TakeshiStraightPunch();
        
    }
}