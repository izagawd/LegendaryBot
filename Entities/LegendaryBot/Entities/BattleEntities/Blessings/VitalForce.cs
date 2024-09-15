using Entities.LegendaryBot.ModifierInterfaces;

namespace Entities.LegendaryBot.Entities.BattleEntities.Blessings;

public class VitalForce : Blessing, IStatsModifier
{
    private const int DefenseIncrease = 10;
    public override string Name => "Vital Force";


    public override Rarity Rarity => Rarity.ThreeStar;


    public override int TypeId
    {
        get => 4;
        protected init { }
    }

    public override string Description => $"Defense is increased by {DefenseIncrease}%";

    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new DefensePercentageModifierArgs(Character!, DefenseIncrease);
    }
}