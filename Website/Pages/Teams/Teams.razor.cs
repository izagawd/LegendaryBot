using Microsoft.AspNetCore.Components;

namespace Website.Pages.Teams;

public partial class Teams : ComponentBase
{
    public class CharacterDto
    {
        public string? Name { get; set; }
        public long Id{ get; set; }
        public int Number{ get; set; }
        public string? ImageUrl{ get; set; }
        public int Level{ get; set; }
    }

    public class TeamCharactersDto
    {
        public CharacterDto[] Characters { get; set; } = [];


        public TeamDto? EquippedTeam => Teams.FirstOrDefault(i => i.Id == EquippedTeamId);
        public long EquippedTeamId { get; set; }
        public TeamDto[] Teams { get; set; } = [];
    }
    
    
    public class CharacterSlot
    {
        public long CharacterId { get; set; }
        
        public int Slot { get; set; }

        public CharacterDto GetCharacter(TeamCharactersDto charactersDto)
        {
            return charactersDto.Characters.First(i => i.Id == CharacterId);
        }
    }
    public class TeamDto
    {
        public IEnumerable<CharacterDto> GetCharacters(TeamCharactersDto teamCharactersDto)
        {
            return CharacterSlots.Select(i => i.GetCharacter(teamCharactersDto));
        }
        public CharacterDto? GetCharacter(int slot, TeamCharactersDto teamCharactersDto)
        {
            return CharacterSlots.FirstOrDefault(i => i.Slot == slot)?.GetCharacter(teamCharactersDto);
        }
        public long Id{ get; set; }
        public List<CharacterSlot> CharacterSlots { get; set; } = [];

        public string? Name{ get; set; }
    }
}