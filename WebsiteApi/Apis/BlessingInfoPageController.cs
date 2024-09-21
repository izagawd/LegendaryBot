using DatabaseManagement;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Blessings;

namespace WebsiteApi.Apis;

[Route("[controller]")]
[ApiController]
public class BlessingInfoPageController : ControllerBase
{
    private readonly PostgreSqlContext _postgre;

    public BlessingInfoPageController(PostgreSqlContext postgreSqlContext)
    {
        _postgre = postgreSqlContext;
    }

    [Authorize]
    [HttpPost("unequip-blessing")]
    public async Task<IActionResult> UnequipBlessing([FromForm] int blessingTypeId, [FromForm] long characterId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await _postgre.Set<UserData>()
            .Include(i =>
                i.Blessings.Where(j => j.TypeId == blessingTypeId && j.CharacterId == characterId))
            .ThenInclude(i => i.Character)
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);
        if (userData is null) return BadRequest("User data not found");

        var blessing =
            userData.Blessings.FirstOrDefault(i => i.TypeId == blessingTypeId && i.Character?.Id == characterId);
        if (blessing is null) return BadRequest("blessing or character not found");

        blessing.Character = null;
        await _postgre.SaveChangesAsync();
        return Ok();
    }

    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult> GetBlessingInfo([FromQuery] int blessingTypeId)
    {
        var discordId = User.GetDiscordUserId();
        var userData = await _postgre.Set<UserData>()
            .AsNoTrackingWithIdentityResolution()
            .Include(i => i.Blessings.Where(j => j.TypeId == blessingTypeId))
            .ThenInclude(i => i.Character)
            .FirstOrDefaultAsync(i => i.DiscordId == discordId);
        if (userData is null) return BadRequest("User data not found");

        var gottenBlessings = userData.Blessings.Where(i => i.TypeId == blessingTypeId).ToArray();
        if (!gottenBlessings.Any()) return BadRequest("You do not have any of that blessing");

        var blessingDto = gottenBlessings.GroupBy(i => i.TypeId).Select(i => new BlessingInfo.BlessingDto
        {
            Name = i.First().Name,
            RarityNum = (int)i.First().Rarity,
            TypeId = i.Key,
            Description = i.First().Description
        }).First();
        var charactersDto = userData.Characters.Where(i => i.Blessing?.TypeId == blessingTypeId)
            .Select(i => new BlessingInfo.CharacterDto
            {
                Id = i.Id,
                RarityNum = (int)i.Rarity,
                TypeId = i.TypeId,
                Name = i.Name
            }).ToList();
        var blessingsInfo = new BlessingInfo.BlessingInfoDto
        {
            Blessing = blessingDto,
            Characters = charactersDto
        };
        return Ok(blessingsInfo);
    }
}