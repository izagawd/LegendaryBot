

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Artifacts;

public class ArtifactStat
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
    public Artifact Artifact { get; set; }
}