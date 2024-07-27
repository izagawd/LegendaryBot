using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Stun : StatusEffect
{

    public override string Description => "Makes affected not able to move";



    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override bool IsStackable => false;
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;

    public override UsageResult OverridenUsage(ref Character target, ref BattleDecision decision, UsageType usageType)
    {
        decision = BattleDecision.Other;
        Affected.CurrentBattle.AddBattleText($"{Affected} cannot move because they are stunned!");
        return new UsageResult(this)
        {
            Text = "dizzy...",
            TargetType = TargetType.None,
            UsageType = usageType,
            User = Affected
        };
    }
}