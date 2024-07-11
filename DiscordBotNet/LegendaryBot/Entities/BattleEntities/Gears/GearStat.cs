

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class GearStat
{
    public Guid Id { get; set; }
    
    public Guid GearId { get; set; }
    public float StatValue { get; set; }
    public StatType StatType { get; set; }
    public bool IsPercentage { get; set; }
    /// <summary>
    /// Will be set to gear owner id instead of null if this GearStat is the main stat
    /// </summary>
    public Guid? IsMainStat { get; set; }
    public Gear Gear { get; set; }
}