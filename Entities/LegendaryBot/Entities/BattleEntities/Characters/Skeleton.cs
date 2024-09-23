using System.ComponentModel.DataAnnotations.Schema;
using BasicFunctionality;
using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class Skeleton : Character
{
    public Skeleton()
    {
        BasicAttack = new SkeletonSwordSlash(this);
    }

    public override int TypeId
    {
        get => 16;
        protected init { }
    }

    [NotMapped] public bool HasDied { get; private set; } = true;

    public override Rarity Rarity => Rarity.TwoStar;
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

    private class SkeletonSwordSlash : BasicAttack
    {
        public SkeletonSwordSlash(Character user) : base(user)
        {
        }

        public override string Name => "Skeleton Sword Slash";

        public override string GetDescription(Character character)
        {
            return "Slashes the opponent, with a 25% chance to inflict bleed";
        }

        protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
            out AttackTargetType attackTargetType,
            out string? text)
        {
            target.Damage(new DamageArgs(User.Attack * 1.7F, new MoveDamageSource(moveUsageContext))
            {
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                ElementToDamageWith = User.Element
            });
            if (BasicFunctions.RandomChance(25)) target.AddStatusEffect(new Bleed(User), User.Effectiveness);

            attackTargetType = AttackTargetType.SingleTarget;
            text = "Skeleton Attack!";
        }
    }
}