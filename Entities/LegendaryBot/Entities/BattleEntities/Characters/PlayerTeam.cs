using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.Models;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class PlayerTeam : CharacterTeam
{
    public PlayerTeam(long userDataId, params Character[] characters) : base(characters)
    {
        UserDataId = userDataId;
    }

    public PlayerTeam(params Character[] characters) : base(characters)
    {
    }

    public PlayerTeam()
    {
    }

    [Timestamp] public uint Version { get; }

    [NotMapped] public bool IsFull => Count >= 4;

    public string TeamName { get; set; } = "Team1";

    public UserData UserData { get; set; }

    /// <summary>
    ///     Will be set to userdata
    /// </summary>
    public long? IsEquipped { get; set; }

    public long Id { get; set; }
    public long UserDataId { get; set; }

    public override bool Add(Character character)
    {
        if (Count >= 4) return false;

        if (UserDataId != 0)
            character.UserDataId = UserDataId;

        return base.Add(character);
    }
}