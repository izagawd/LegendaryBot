using System.Linq.Expressions;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public class GiveMe : GeneralCommandClass
{

    [SlashCommand("give_me", "Gets daily rewards"),
     AdditionalSlashCommand("/give_me", BotCommandType.Battle)]
    public async Task Execute(InteractionContext ctx,[Option("entity_name","the name of entity you want")]
        string entityName,[Option("entity_amount","the amount")] long amount = 1)
    {
        var simplifiedEntityName = entityName.ToLower().Replace(" ", "");
        var type =DefaultObjects.AllAssemblyTypes.FirstOrDefault(i => i.IsSubclassOf(typeof(Entity)) && !i.IsAbstract
            && i.Name.ToLower() == simplifiedEntityName);
        if (type is null)
        {
            await ctx.CreateResponseAsync("invalid item inputted");
        } else if (amount < 1)
        {
            await ctx.CreateResponseAsync("amount must be at least 1");
        }
        else
        {
            var userData = await DatabaseContext.UserData
                .Include(i => i.Inventory)
                .FindOrCreateUserDataAsync((long)ctx.User.Id);
            Entity createdType = ((Entity)DefaultObjects.GetDefaultObject(type)).Clone();
            if (createdType is Character && userData.Inventory.Any(i => i.GetType() == type))
            {
                await ctx.CreateResponseAsync("sorry, cant have dupe characters");
                return;
            } 
            if (createdType is Item item)
            {
                item.Stacks =(int) amount;
                if(userData.Inventory.Any(i => i.GetType() == type))
                {
                    var gottenItem = userData.Inventory.OfType<Item>().First(i => i.GetType() == type);
                    gottenItem.Stacks += (int) amount;
                }
                else
                {
                    userData.Inventory.Add(item);
                }
            }
            else
            {
                if(createdType is Gear gear)
                    gear.Initialize(Rarity.FiveStar);
                userData.Inventory.Add(createdType);
            }

            await DatabaseContext.SaveChangesAsync();
            await ctx.CreateResponseAsync("Done!");
        }
    }
}