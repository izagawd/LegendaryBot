using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class Jock : Character
{
    class BallThrow : BasicAttack
    {
        public BallThrow(Character user) : base(user)
        {
        }

        public override string GetDescription(Character character)
        {
            return
                "Throws a ball at the target, with a 25% chance for the ball to come back to the user, making the user throw it a the target again";
        }

        protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
            out string? text)
        {
            attackTargetType = AttackTargetType.SingleTarget;
            target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                ElementToDamageWith = User.Element,
                DamageText = $"{User.NameWithAlphabet} throws a foot ball at {target.NameWithAlphabet}, dealing $ damage"
            });
            text = "heh...";
        }


        public override string Name => "Ball Throw";
    }

    public override string Name => "Jock";

    [BattleEventListenerMethod]
    public void RedoBallThrow(CharacterPostUseMoveEventArgs postUseMoveEventArgs)
    {
        if (postUseMoveEventArgs.Move == BasicAttack && !CannotDoAnything)
        {
            BasicAttack.Utilize(postUseMoveEventArgs.MoveUsageResult.DamageResults.First().DamageReceiver,
                MoveUsageType.MiscellaneousFollowUpUsage);
        }
    }
    public override Rarity Rarity => Rarity.TwoStar;
    public override int TypeId
    {
        get => 17;
        protected init {}
    }

    public Jock()
    {
        BasicAttack = new BallThrow(this);
    }
}