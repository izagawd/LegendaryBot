using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.Pages.team;


[Authorize]
public class Index : PageModel
{
    
    
    public UserData UserData { get; private set; }

    public string[] TeamNames { get; protected set; } = [];
    private PostgreSqlContext DatabaseContext { get;set; }

    public Index(PostgreSqlContext context)
    {
        DatabaseContext = context;
    }
    public async Task OnPostAsync(string teamName)
    {
        await OnGetAsync();
        var gottenTeam = await DatabaseContext.Set<PlayerTeam>()
            .Where(i => i.TeamName == teamName && i.UserDataId == UserData.Id)
            .FirstOrDefaultAsync();

        if (gottenTeam is not null)
        {
            UserData.EquippedPlayerTeam = gottenTeam;
            await DatabaseContext.SaveChangesAsync();
        }
    }
    public async Task OnGetAsync()
    {
        var anonymous = await DatabaseContext.UserData
            .Include(i => i.EquippedPlayerTeam)
            .Include(j => j.Inventory.Where(k => k is Character))
            
            .FindOrCreateSelectUserDataAsync(User.GetDiscordUserId(),
                i => new{UserData = i,TeamNames= i.PlayerTeams.Select(j => j.TeamName)});
        UserData = anonymous.UserData;
        TeamNames = anonymous.TeamNames.ToArray();



    }


}