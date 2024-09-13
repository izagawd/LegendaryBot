using Entities.LegendaryBot.ModifierInterfaces;

namespace Entities.LegendaryBot.Entities.BattleEntities.Blessings;

public class VitalForce : Blessing, IStatsModifier
{
    public override string Name => "Vital Force";


    public override Rarity Rarity => Rarity.ThreeStar;


    public override int TypeId
    {
        get => 4;
        protected init { }
    }

    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new DefensePercentageModifierArgs(Character!, DefenseIncrease);
    }

    public override string Description => $"Defense is increased by {DefenseIncrease}%";
    

    private const int DefenseIncrease = 10;

}