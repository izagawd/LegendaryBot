namespace Entities.LegendaryBot.ModifierInterfaces;

public interface IStatsModifier
{
    /// <returns></returns>
    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs();
}