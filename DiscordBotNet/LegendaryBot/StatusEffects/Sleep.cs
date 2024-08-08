using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Sleep: StatusEffect
{

    public override string Name => "Sleep";
    public override string Description =>
        "Makes affected not able to move. Is dispelled when affected takes damage from a move";
    public override bool IsStackable => false;

    public override OverrideTurnType OverrideTurnType => OverrideTurnType.CannotMove;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;

    [BattleEventListenerMethod]
    public void PostDamageEvent(CharacterPostDamageEventArgs eventArgs)
    {
        if(eventArgs.DamageResult.DamageReceiver != Affected || eventArgs.DamageResult.DamageDealt <= 0) return;
     
        var removed = Affected.RemoveStatusEffect(this);
        if(removed)
            Affected.CurrentBattle.AddBattleText($"{this} has been dispelled from {Affected.NameWithAlphabet} due to taking damage!");
    }

    public override string? OverridenUsage(ref Character target, ref BattleDecision decision,
        MoveUsageType moveUsageType)
    {
        Affected.CurrentBattle.AddBattleText($"{Affected} is fast asleep");
        return "Snores...";
    }

    public Sleep(Character caster) : base(caster)
    {
    }
}