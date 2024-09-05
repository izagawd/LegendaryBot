using Microsoft.AspNetCore.Components;

namespace Website.Pages.Teams;

public partial class Teams : ComponentBase
{
    public class CharacterDto
    {
        public string Name { get; set; }
        public long Id{ get; set; }
        public int Number{ get; set; }
        public string ImageUrl{ get; set; }
        public int Level{ get; set; }
    }

    public class TeamCharactersDto
    {
        public CharacterDto[] Characters { get; set; } = [];


        public TeamDto EquippedTeam => Teams.FirstOrDefault(i => i.Id == EquippedTeamId);
        public long EquippedTeamId { get; set; }
        public TeamDto[] Teams { get; set; } = [];
    }
    public class TeamDto
    {
        public IEnumerable<CharacterDto> GetCharacters(TeamCharactersDto teamCharactersDto)
        {
            return teamCharactersDto.Characters.Where(i => GottenCharacters.Contains(i.Id));
        }
        public long Id{ get; set; }
        public List<long> GottenCharacters { get; set; } = [];

        public string Name{ get; set; }
    }
}