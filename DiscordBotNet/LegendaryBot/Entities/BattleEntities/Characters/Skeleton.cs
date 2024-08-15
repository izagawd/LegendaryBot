using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Functionality;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class Skeleton : Character
{
    class SkeletonSwordSlash : BasicAttack
    {
        public SkeletonSwordSlash(Character user) : base(user)
        {
        }

        public override string GetDescription(Character character)
        {
            return "Slashes the opponent, with a 25% chance to inflict bleed";
        }

        protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
            out string? text)
        {
            target.Damage(new DamageArgs(User.Attack * 1.7F, new MoveDamageSource(moveUsageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                ElementToDamageWith = User.Element,

            });
            if (BasicFunctionality.RandomChance(25))
            {
                target.AddStatusEffect(new Bleed(User),User.Effectiveness);
            }

            attackTargetType = AttackTargetType.SingleTarget;
            text = "Skeleton Attack!";
        }

        public override string Name => "Skeleton Sword Slash";
    }
    public override int TypeId
    {
        get => 16;
        protected init {}
    }

    public Skeleton()
    {

        BasicAttack = new SkeletonSwordSlash(this);
    }
    [NotMapped]
    public bool HasDied { get; private set; } = true;
    public override string Name => "Skeleton";

    [BattleEventListenerMethod]
    public void ReviveOnDeath(CharacterPostUseMoveEventArgs eventArgs)
    {
        if (!HasDied && IsDead)
        {
            HasDied = true;
            Revive();
        }
    }
    
}