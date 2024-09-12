using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
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
    private readonly PostgreSqlContext context;

    public TeamPageController(PostgreSqlContext databaseContext)
    {
        context = databaseContext;
    }

    [Authorize]
    [HttpPost("rename-team")]
    public async Task<IActionResult> RenameTeamAsync([FromForm] long teamId, [FromForm] string newName)
    {
        if (newName.Length <= 0)
            return BadRequest("Name length must be at least 1 character");
        var userId = User.GetDiscordUserId();
        var didDo = await context.Set<PlayerTeam>()
            .Where(i => i.UserData.DiscordId == userId && i.Id == teamId
                                                       && i.UserData.PlayerTeams.All(j =>
                                                           j.TeamName.ToLower() != newName.ToLower()))
            .ExecuteUpdateAsync(i => i.SetProperty(j => j.TeamName, newName));
        if (didDo <= 0)
            return BadRequest("One of your teams already has that name");
        return Ok();
    }

    [Authorize]
    [HttpGet("get-teams-and-characters")]
    public async Task<IActionResult> GetSomeUserData()
        {

            var discordId = User.GetDiscordUserId();
            var userData = await context.Set<UserData>()
                .Include(i => i.PlayerTeams)
                .ThenInclude(i => i.TeamMemberships)
                .ThenInclude(i => i.Character)
                .FirstOrDefaultAsync(i => i.DiscordId == discordId);



            if (userData is null)
                return BadRequest(
                    "No team was found in database. You may not have began your journey with /begin with the discord bot");
  


            var dto = new Teams.TeamCharactersDto
            {
                Characters = userData.Characters.Select(k => new Teams.CharacterDto
                {
                    Id = k.Id,
                    ImageUrl = k.ImageUrl,
                    Level = k.Level,
                    Name = k.Name,
                    Number = k.Number,
                    RarityNum = (int)k.Rarity
                }).ToArray(),
                Teams = userData.PlayerTeams.Select(j => new Teams.TeamDto
                {
                    Id = j.Id,
                    CharacterSlots = j.TeamMemberships.Select(k =>
                        new Teams.CharacterSlot
                        {
                            CharacterId = k.CharacterId,
                            Slot = k.Slot
                        }).ToList(),
                    Name = j.TeamName
                }).ToArray(),
                EquippedTeamId = userData.EquippedPlayerTeam!.Id
            };

            return Ok(dto);
        }

    [HttpPost("switch-slots")]
    [Authorize]
    public async Task<IActionResult> SwitchSlotAsync([FromForm] int slot1, [FromForm] int slot2, [FromForm] long teamId)
    {
        int[] slots = [slot1, slot2];
        foreach (var slot in slots)
            if (slot < 1 || slot > TypesFunction.GetDefaultObject<PlayerTeam>().MaxCharacters)
                return BadRequest("Slot input out of range. Range is 1-4");

        var theId = User.GetDiscordUserId();
        var userData = await context.Set<UserData>()
            .Include(i => i.PlayerTeams)
            .ThenInclude(i => i.TeamMemberships)
            .ThenInclude(i => i.Character)
            .FirstOrDefaultAsync(i => i.DiscordId == theId);

        if (userData is null)
            return BadRequest("Something went wrong");
        var theTeam = userData.PlayerTeams.FirstOrDefault(i => i.Id == teamId);
        if (theTeam is null)
            return BadRequest("Something went wrong");
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                var slotOneCharacter = theTeam[slot1];
                theTeam[slot1] = theTeam[slot2];
                await context.SaveChangesAsync();
                theTeam[slot2] = slotOneCharacter;
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return Ok();
    }

    [HttpPost("set-slot")]
    [Authorize]
    public async Task<IActionResult> SetSlotAsync([FromForm] int slot, [FromForm] long teamId,
        [FromForm] long characterId)
    {
        if (slot < 1 || slot > TypesFunction.GetDefaultObject<PlayerTeam>().MaxCharacters)
            return BadRequest("Slot input out of range. Range is 1-4");

        var theId = User.GetDiscordUserId();
        var userData = await context.Set<UserData>()
            .Include(i => i.PlayerTeams)
            .ThenInclude(i => i.TeamMemberships)
            .ThenInclude(i => i.Character)
            .Include(i => i.Characters.Where(j => j.Id == characterId))
            .FirstOrDefaultAsync(i => i.DiscordId == theId);

        if (userData is null)
            return BadRequest("Something went wrong");
        var theCharacter = userData.Characters.FirstOrDefault(i => i.Id == characterId);
        if (theCharacter is null)
            return BadRequest("Something went wrong");
        var theTeam = userData.PlayerTeams.FirstOrDefault(i => i.Id == teamId);
        if (theTeam is null)
            return BadRequest("Something went wrong");
        theTeam[slot] = theCharacter;

        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("remove-from-team")]
    [Authorize]
    public async Task<IActionResult> RemoveFromTeamAsync([FromForm] int slot, [FromForm] long teamId)
    {
        if (slot < 1 || slot > TypesFunction.GetDefaultObject<PlayerTeam>().MaxCharacters)
            return BadRequest("Inputted slot out of range. range is 1-4");

        var theId = User.GetDiscordUserId();
        var deleted = await context.Set<PlayerTeamMembership>()
            .Where(i => i.Character.UserData!.DiscordId == theId && i.PlayerTeamId == teamId && i.Slot == slot
                        && i.PlayerTeam.TeamMemberships.Count > 1)
            .ExecuteDeleteAsync();

        if (deleted < 1)
            return BadRequest("Something went wrong");
        return Ok();
    }

    [Authorize]
    [HttpPost("equip-team")]
    public async Task<IActionResult> EquipTeam([FromForm] long teamId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await context.Set<UserData>()
            .Include(i => i.PlayerTeams)
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);

        if (userData is null)
            return BadRequest("Something went wrong");


        userData.EquippedPlayerTeam = userData.PlayerTeams.FirstOrDefault(i => i.Id == teamId);
        if (userData.EquippedPlayerTeam is null)
            return BadRequest("Something went wrong");
        await context.SaveChangesAsync();

        return Ok();
    }
}