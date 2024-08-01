using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Stun : StatusEffect
{
    public override string Name => "Stun";
    public override string Description => "Makes affected not able to move";



    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    
    public override bool IsStackable => false;
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;

    public override string? OverridenUsage(ref Character target, ref BattleDecision decision,
        MoveUsageType moveUsageType)
    {
        decision = BattleDecision.Other;
        Affected.CurrentBattle.AddBattleText($"{Affected} cannot move because they are stunned!");
        return "dizzy...";
    }
}