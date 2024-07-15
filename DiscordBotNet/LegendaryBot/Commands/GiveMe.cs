using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public class GiveMe : GeneralCommandClass
{

    [Command("give_me"),
     AdditionalCommand("/give_me", BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx,[Parameter("entity_name")]
        string entityName,[Parameter("entity_amount"), 
                           Description("The amount you want of the supplied item")] long amount = 1)
    {
        var simplifiedEntityName = entityName.ToLower().Replace(" ", "");
        var type =DefaultObjects.AllAssemblyTypes.FirstOrDefault(i => i.IsSubclassOf(typeof(Entity)) && !i.IsAbstract
            && i.Name.ToLower() == simplifiedEntityName);
        if (type is null)
        {
            await ctx.RespondAsync("invalid item inputted");
        } else if (amount < 1)
        {
            await ctx.RespondAsync("amount must be at least 1");
        }
        else
        {
            var userData = await DatabaseContext.UserData
                .Include(i => i.Inventory)
                .FindOrCreateUserDataAsync(ctx.User.Id);
            var createdType = ((Entity)DefaultObjects.GetDefaultObject(type)).Clone();
            if (createdType is Character && userData.Inventory.Any(i => i.GetType() == type))
            {
                await ctx.RespondAsync("sorry, cant have dupe characters");
                return;
            } 
            if(createdType is Gear gear)
                    gear.Initialize(Rarity.FiveStar);
            userData.Inventory.Add(createdType);
            await DatabaseContext.SaveChangesAsync();
            await ctx.RespondAsync("Done!");
        }
    }
}