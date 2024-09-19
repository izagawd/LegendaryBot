using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class Jock : Character
{
    public Jock()
    {
        BasicAttack = new BallThrow(this);
    }

    public override string Name => "Jock";
    public override Rarity Rarity => Rarity.TwoStar;

    public override int TypeId
    {
        get => 17;
        protected init { }
    }

    [BattleEventListenerMethod]
    public void RedoBallThrow(CharacterPostUseMoveEventArgs postUseMoveEventArgs)
    {

        if (postUseMoveEventArgs.Move == BasicAttack && !CannotDoAnything &&
            postUseMoveEventArgs.MoveUsageResult.MoveUsageType == MoveUsageType.NormalUsage)
        {
            var damageResult = postUseMoveEventArgs.MoveUsageResult.DamageResults.FirstOrDefault();
            if (!(damageResult?.DamageReceiver.IsDead ?? true))
            {
                BasicAttack.Utilize(postUseMoveEventArgs.MoveUsageResult.DamageResults.First().DamageReceiver,
                    MoveUsageType.MiscellaneousFollowUpUsage);
            }
   
        }
           
    }

    private class BallThrow : BasicAttack
    {
        public BallThrow(Character user) : base(user)
        {
        }


        public override string Name => "Ball Throw";

        public override string GetDescription(Character character)
        {
            return
                "Throws a ball at the target, with a 25% chance for the ball to come back to the user, making the user throw it a the target again";
        }

        protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
            out AttackTargetType attackTargetType,
            out string? text)
        {
            attackTargetType = AttackTargetType.SingleTarget;
            target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                ElementToDamageWith = User.Element,
                DamageText =
                    $"{User.NameWithAlphabet} throws a foot ball at {target.NameWithAlphabet}, dealing $ damage"
            });
            
            text = "heh...";
        }
    }
}