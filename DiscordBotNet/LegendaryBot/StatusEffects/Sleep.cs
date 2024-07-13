using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Sleep: StatusEffect, IBattleEventListener
{
    public override string Description =>
        "Makes affected not able to move. Is dispelled when affected takes damage from a move";
    public override int MaxStacks => 1;

    public Sleep(Character caster) : base(caster)
    {
        
    }
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    [BattleEventListenerMethod]
    public void OnTurnStart(CharacterPostDamageEventArgs eventArgs)
    {
        if(eventArgs.DamageResult.DamageReceiver != Affected) return;
        if (eventArgs.DamageResult.StatusEffect is not null) return;
        var removed = Affected.RemoveStatusEffect(this);
        if(removed)
            Affected.CurrentBattle.AddAdditionalBattleText($"{this} has been dispelled from {Affected.NameWithAlphabetIdentifier} due to an attack!");
    }

    public override UsageResult OverridenUsage(Character affected, ref Character target, ref BattleDecision decision, UsageType usageType)
    {
        affected.CurrentBattle.AddAdditionalBattleText($"{affected} is fast asleep");
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.None,
            Text = "Snores...",
            User = affected
        };

    }
}