using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.Pages.blessings;

public class Index : PageModel
{
    public string Name { get; set; }

    public string Sort { get; set; }
    public Blessing[] Blessings { get; set; }

    public PostgreSqlContext DatabaseContext { get; set; }
    public Index(PostgreSqlContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
  
    public async Task OnGetAsync(string name, string sort)
    { 
        
       
        if (name is null)
        {
            name = "";
        }

        Name = name;
        if (sort is null)
        {
            sort = "level";
        }
        Sort = sort;



        var userData = await DatabaseContext.UserData
            .Include(j => j.Inventory.Where(k => k is Blessing))
            .FindOrCreateUserDataAsync(User.GetDiscordUserId());

        Blessings = userData.Inventory.OfType<Blessing>().ToArray();

    }
}