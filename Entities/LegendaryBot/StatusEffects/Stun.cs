using Entities.LegendaryBot.BattleSimulatorStuff;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.StatusEffects;

public class Stun : StatusEffect
{
    public Stun(CharacterPartials_Character caster) : base(caster)
    {
    }

    public override string Name => "Stun";
    public override string Description => "Makes affected not able to move";


    public override StatusEffectType EffectType => StatusEffectType.Debuff;


    public override bool IsStackable => false;
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;

    public override string? OverridenUsage(ref CharacterPartials_Character target, ref BattleDecision decision,
        MoveUsageType moveUsageType)
    {
        decision = BattleDecision.Other;
        Affected.CurrentBattle.AddBattleText($"{Affected} cannot move because they are stunned!");
        return "dizzy...";
    }
}