using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.Pages.characters;

[Authorize]
public class Index : PageModel
{

    public Element? Element { get; set; }
    public string Name { get; set; }

    public string Sort { get; set; }
    public Character[] Characters { get; set; }

    public PostgreSqlContext DatabaseContext { get; set; }
    public Index(PostgreSqlContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }

    public async Task OnGetAsync(Element? element,string name, string sort)
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
        Element = element;


        var userData = await DatabaseContext.UserData
            .Include(j => j.Inventory.Where(k => k is Character))
            .FindOrCreateUserDataAsync(User.GetDiscordUserId());

        Characters = userData.Inventory.OfType<Character>().ToArray();

    }
}
