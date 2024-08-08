using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Moves;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.GearSets;

public class SimpleAttackSet : GearSet, IStatsModifier
{
    private const int AttackIncreaseAttackAmount = 15;
    private const int BasicAttackIncreaseAttackAmount = 15;
    public override string Name => "Simple Attack Set";
    public override string TwoPieceDescription => $"Increases attack by {AttackIncreaseAttackAmount}%";

    public override string FourPieceDescription =>
        $"Increases damage dealt by Basic Attacks by {BasicAttackIncreaseAttackAmount}%";

    public SimpleAttackSet()
    {
        TypeId = 1;
    }


    [BattleEventListenerMethod]
    public void OnDamaging(CharacterPreDamageEventArgs preDamageEventArgs)
    {
        if (CanUseFourPiece && preDamageEventArgs.DamageArgs.DamageSource is MoveDamageSource moveDamageSource
            && moveDamageSource.Move.User == Owner && moveDamageSource.Move is BasicAttack)
        {
            preDamageEventArgs.DamageArgs.Damage *= 1 + (BasicAttackIncreaseAttackAmount * 0.01f);
        }
            
    }

    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new AttackPercentageModifierArgs(Owner, AttackIncreaseAttackAmount);
    }
}