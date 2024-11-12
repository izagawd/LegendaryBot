using Entities.Models;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class PlayerTeam : Team<PlayerTeamMembership>
{
    public long Id { get; set; }
    public string TeamName { get; set; } = "Team1";
    public long? IsEquipped { get; set; }
    public long UserDataId { get; set; }
    public UserData UserData { get; set; }

    public override int MaxCharacters
    {
        get => 4;
        set { }
    }

}