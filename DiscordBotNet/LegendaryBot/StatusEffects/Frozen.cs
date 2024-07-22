using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Frozen : StatusEffect
{
    public override string Description => "Makes affected not able to move";

    


    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    public override int MaxStacks => 1;
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;

    public override UsageResult OverridenUsage(ref Character target, ref BattleDecision decision, UsageType usageType)
    {
        decision = BattleDecision.Other;
        Affected.CurrentBattle.AddAdditionalBattleText($"{Affected} cannot move because they are frozen!");
        return new UsageResult(this)
        {
            Text = "c-cold...",
            UsageType = usageType,
            User = Affected,
            TargetType = TargetType.None
        };
    }
}