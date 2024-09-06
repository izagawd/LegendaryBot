using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public class NonPlayerTeam : Team<TeamMembership>
{

    public NonPlayerTeam(params Character[] characters) : this(characters.AsEnumerable())
    {
        
    }
    public NonPlayerTeam(IEnumerable<Character> characters) : this()
    {
        foreach (var i in characters)
        {
            Add(i);
           
        }
    }
    public NonPlayerTeam()
    {
        
    }
    public override int MaxCharacters { get; set; } = int.MaxValue;
}