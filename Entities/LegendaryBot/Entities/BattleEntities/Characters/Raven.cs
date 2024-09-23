using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;
using  Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;





public class Raven : Character
{

    public class PositionalAdvantage : Skill
    {
        public override bool IsPassive => true;

        public PositionalAdvantage(Character user) : base(user)
        {
        }

        public override string Name => "Positional Advantage";

        public const int MinimumSuperPoints = 5;
        public override string GetDescription(Character character)
        {
            return $"When superpoints reaches {MinimumSuperPoints}, gets an extra turn";
        }

        public override IEnumerable<Character> GetPossibleTargets()
        {
            return [];
        }

        protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
            out string? text)
        {
            throw new NotImplementedException();
        }

        public override int MaxCooldown => 0;
    }
    public class ShotGunShot : Ultimate
    {
        public ShotGunShot(Character user) : base(user)
        {
        }

        public override string Name => "Shot Gun Shot";
        public override string GetDescription(Character character)
        {
            return "Shoots a shotgun at multiple targets, then increases super points by 5!";
        }

        public override IEnumerable<Character> GetPossibleTargets()
        {
            return CurrentBattle.Characters.Where(i => i.BattleTeam != User.BattleTeam && !i.IsDead);
        }

        protected override void UtilizeImplementation(Character target, 
            MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
            out string? text)
        {
            foreach (var i in GetPossibleTargets())
            {
                i.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
                {
                    ElementToDamageWith = User.Element,
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                
                    DamageText = $"{User} shotguns {target}, dealing $ damage!"
                });
            }

            User.SuperPoints += 5;
            attackTargetType = AttackTargetType.AOE;
            text  = "Shotto Gun!";
        }

        public override int MaxCooldown => 4;
    }
    public class BleedingShot : BasicAttack
    {
        public BleedingShot(Character user) : base(user)
        {
        }

        public override string Name => "Bleeding Shot";
        private const int BleedChance = 75;
        public override string GetDescription(Character character)
        {
            return $"Shoots the enemy, with a {BleedChance}% chance to inflict bleed for 1 turn. Increases superpoints by 1-2";
        }

        protected override void UtilizeImplementation(Character target,
            MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType,
            out string? text)
        {
            target.Damage(new DamageArgs(User.Attack * 1.7f, new MoveDamageSource(moveUsageContext))
            {
                ElementToDamageWith = User.Element,
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                
                DamageText = $"{User} shoots {target}, dealing $ damage!"
            });
            if (BasicFunctionality.BasicFunctions.RandomChance(BleedChance))
            {
                target.AddStatusEffect(new Bleed(User) { Duration = 1 },
                    User.Effectiveness);
            }

            
            attackTargetType = AttackTargetType.SingleTarget;
            text = "Shot!!!";
            User.SuperPoints += BasicFunctionality.BasicFunctions.RandomChoice([1, 2]);
        }
    }

    public override Rarity Rarity => Rarity.FourStar;

    public Raven()
    {
        BasicAttack = new BleedingShot(this);
        Skill = new PositionalAdvantage(this);
        Ultimate = new ShotGunShot(this);
    }

    public override bool UsesSuperPoints => true;

    [BattleEventListenerMethod]
    public void ExtraTurnGrant(CharacterPostUseMoveEventArgs eventArgs)
    {
        if (!IsDead && SuperPoints >= PositionalAdvantage.MinimumSuperPoints)
        {
            SuperPoints -= PositionalAdvantage.MinimumSuperPoints;
            GrantExtraTurn();
            
        }
    }
    public override int TypeId
    {
        get => 19; protected init{}}

    public override string Name => "Raven";

   
}