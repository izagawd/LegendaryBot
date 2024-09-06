using Entities.Models;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class PlayerTeam : Team<PlayerTeamMembership>
{
    public long Id { get; private set; }
    public string TeamName { get; set; } = "Team1";
    public string IncreaseExp(int expToGain)
    {
        expToGain = expToGain / Count;
        var str = "";
        foreach (var i in this)
        {
            str +=  $"{i.IncreaseExp(expToGain)}\n";
        }

        return str;
    }
    public long? IsEquipped { get; set; }
    public long UserDataId { get; set; }
    public UserData UserData { get; set; }
    public override int MaxCharacters
    {
        get => 4; set{} }
}