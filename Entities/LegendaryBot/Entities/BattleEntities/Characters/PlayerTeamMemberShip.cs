using DatabaseManagement;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class PlayerTeamMembership : TeamMembership
{
    public PlayerTeam PlayerTeam { get; set; }
    public long CharacterId { get; set; }
    public long PlayerTeamId { get; set; }
}