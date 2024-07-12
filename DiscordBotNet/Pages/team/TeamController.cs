using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.Pages.team;

public class TeamController : Controller
{
    private PostgreSqlContext DatabaseContext { get; }
    public TeamController(PostgreSqlContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }


    
    [HttpPost]
    public async Task<IActionResult> RemoveFromTeamAsync([FromBody]string idString)
    {
        var id = Guid.Parse(idString);
        var UserData = await DatabaseContext.UserData
            .IncludeTeam()
            .FindOrCreateUserDataAsync(User.GetDiscordUserId());

        var userTeam = UserData.EquippedPlayerTeam;
        if (userTeam.Count <= 1) return Ok();
        var characterToRemove = userTeam.FirstOrDefault(i => i.Id == id);
        
        if (characterToRemove is not null)
        {
            UserData.EquippedPlayerTeam?.Remove(characterToRemove);
        }
        
        await DatabaseContext.SaveChangesAsync();

        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> AddToTeamAsync([FromBody]string idString)
    {
        var id = Guid.Parse(idString);
        var userData = await DatabaseContext.UserData
            .Include(j => j.Inventory.Where(k => k.Id == id)).IncludeTeam()
            .FindOrCreateUserDataAsync(User.GetDiscordUserId());
        var character = userData.Inventory.OfType<Character>().FirstOrDefault(i => i.Id == id);
        
        if (character is not null)
        {
            userData.EquippedPlayerTeam.Add(character);
        }
        await DatabaseContext.SaveChangesAsync();
        return Ok();
    }

}