using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.Items;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Teams;

namespace WebsiteApi.Apis;
[Route("[controller]")]
[ApiController] 
public class TeamPageController : ControllerBase
{
    private static readonly int PlayerTypeId = TypesFunction.GetDefaultObject<Player>().TypeId;

    [Authorize]
    [HttpGet("get-teams-and-characters")]
    public async Task<IActionResult> GetSomeUserData()
    {

        await using var context = new PostgreSqlContext();
        var discordId = User.GetDiscordUserId();
        var selected = await context.UserData
            .Where(i => i.DiscordId == discordId)
            .Select(i => new Teams.TeamCharactersDto()
            {
                Characters = i.Characters.Select(k => new Teams.CharacterDto()
                {
                    Id = k.Id,
                    ImageUrl = k.TypeId == PlayerTypeId ?
                        Player.GetImageUrl(i.Gender) : 
                        Character.GetDefaultFromTypeId(k.TypeId).ImageUrl,
                    Level = k.Level,
                    Name = k.TypeId == PlayerTypeId ? i.Name :
                        Character.GetDefaultFromTypeId(k.TypeId).Name,
                    Number = k.Number
                }).ToArray(),
                Teams = i.PlayerTeams.Select(j => new Teams.TeamDto()
                {
                    Id = j.Id,
                    GottenCharacters = j.Characters.Select(k => k.Id).ToList(),
                    Name = j.TeamName
                }).ToArray(),
                EquippedTeamId = i.EquippedPlayerTeam.Id
            }).FirstOrDefaultAsync();
        
        return Ok(selected ?? new Teams.TeamCharactersDto());
    }

    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddCharacterToTeam([FromBody] long teamId, 
       [FromBody] long characterId)
    {
        
        await using var context = new PostgreSqlContext();
        var discordId = User.GetDiscordUserId();
        var team = await context.UserData.Where(i => i.DiscordId == discordId)
            .SelectMany(i => i.PlayerTeams)
            .Include(i => i.Characters.Where(j => j.Id == characterId))
            .FirstOrDefaultAsync(i => i.Id == teamId);
        if (team is null)
            return BadRequest($"No team with Id {teamId} could be found");
        var zaFirst = team.Characters.FirstOrDefault(i => i.Id == characterId);
        if (zaFirst is null)
        {
            return BadRequest($"Team doesnt have character with Id {characterId}");
        }

        team.Remove(zaFirst);
        await context.SaveChangesAsync();
        return Ok();
    }
    
}