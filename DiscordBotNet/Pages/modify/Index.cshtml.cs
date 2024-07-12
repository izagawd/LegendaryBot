using System.Reflection;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.Pages.modify;
[Authorize]
public class Index : PageModel
{

    private static Type[] _addableTypes;

    public static Type[] AddableTypes => _addableTypes.ToArray();
    static Index()
    {
        _addableTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(i => i.IsSubclassOf(typeof(Entity)) && !i.IsAbstract)
            .ToArray();
    }
    public Index(PostgreSqlContext context)
    {
        DatabaseContext = context;
    }
    public PostgreSqlContext DatabaseContext { get; private set; }

    public async Task OnPostAddAsync(string? type)
    {

        Type theType = AddableTypes.First(i => i.Name == type);
        Entity entity = (Entity)Activator.CreateInstance(theType)!;

        entity.UserDataId = User.GetDiscordUserId();

        await DatabaseContext.Entity.AddAsync(entity);


    
        await DatabaseContext.SaveChangesAsync();
    }

    public async Task OnPostStrengthenAsync()
    {


        await DatabaseContext.SaveChangesAsync();
    }
    /// <summary>
    /// Categorizes an entity into blessing, gear, or character
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>The type it is categorized as</returns>
    public Type Categorize(Type entityType)
    {

        if (entityType.IsRelatedToType(typeof(Character))) return typeof(Character);
        if (entityType.IsRelatedToType(typeof(Blessing))) return typeof(Blessing);
        return typeof(Entity);
    }
}