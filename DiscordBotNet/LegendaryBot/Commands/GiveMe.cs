using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class GiveMe : GeneralCommandClass
{

    [Command("give-me"),
     AdditionalCommand("/give-me", BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx,[Parameter("entity-name")]
        string entityName,[Parameter("entity-amount"), 
                           Description("The amount you want of the supplied item")] long amount = 1)
    {
        var simplifiedEntityName = entityName.ToLower().Replace(" ", "");
        var type =DefaultObjects.AllAssemblyTypes.FirstOrDefault(i => i.IsClass 
                                                                      && i.GetInterfaces().Contains(typeof(IInventoryEntity)) && !i.IsAbstract
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
                .Include(i => i.Gears)
                .Include(i => i.Blessings)
                .Include(i => i.Characters)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == ctx.User.Id);
            if (userData is null || userData.Tier == Tier.Unranked)
            {
                await AskToDoBeginAsync(ctx);
                return;
            }
            List<EntityReward> rewards = [];
            foreach (var i in Enumerable.Range(0,(int) amount))
            {
                var createdType = (IInventoryEntity) Activator.CreateInstance(type)!;
                if(createdType is Gear gear)
                    gear.Initialize(Rarity.FiveStar);
                rewards.Add(new EntityReward([createdType]));
                
            }
            var result = userData.ReceiveRewards(rewards);
            await DatabaseContext.SaveChangesAsync();
            var embed = new DiscordEmbedBuilder()
                .WithUser(ctx.User)
                .WithColor(userData.Color)
                .WithTitle("Success!")
                .WithDescription(result);
            await ctx.RespondAsync(embed);
            
        }
    }
}