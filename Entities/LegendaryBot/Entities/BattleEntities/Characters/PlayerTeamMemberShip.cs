using DatabaseManagement;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class PlayerTeamMembership : TeamMembership
{
    public long CharacterId { get; set; }
    public long PlayerTeamId { get; set; }
}