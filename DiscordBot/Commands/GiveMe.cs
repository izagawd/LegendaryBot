using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class GiveMe : GeneralCommandClass
{
    [Command("give-someone")]
    [Description("Use this to give anything to someone")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public  ValueTask ExecuteGiveSomeone(CommandContext ctx,[Parameter("user-to-give")] DiscordUser toGive, [Parameter("entity-name")] string entityName,
        [Parameter("entity-amount")] [Description("The amount you want of the supplied item")]
        int amount = 1)
    {
        return GiveSomeone(ctx, toGive, entityName, amount);
    }

    public async ValueTask GiveSomeone(
        CommandContext ctx, DiscordUser userToGive, string entityName, int amount)
    {
        
                var simplifiedEntityName = entityName.ToLower().Replace(" ", "");
                var caller = ctx.User.Id;
        var type = TypesFunction
            .GetDefaultObjectsAndSubclasses<IInventoryEntity>()
            .FirstOrDefault(i => i.Name.ToLower().Replace(" ", "") == simplifiedEntityName)?.GetType();
        if (caller != DiscordBot.Izasid && caller != DiscordBot.Testersid)
        {
            await ctx.RespondAsync("Only izagawd can use this command");
        }
        else if (type is null)
        {
            await ctx.RespondAsync("invalid item inputted");
        }
        else if (amount < 1)
        {
            await ctx.RespondAsync("amount must be at least 1");
        }
        else
        {
            var userData = await DatabaseContext.Set<UserData>()
                .FirstOrDefaultAsync(i => i.DiscordId == userToGive.Id);
            if (userData is null)
            {
                userData = await DatabaseContext.CreateNonExistantUserdataAsync(userToGive.Id);
            }

            if (userData.IsOccupied)
            {
                await ctx.RespondAsync($"{userToGive.Username} is occupied");
                return;
            }

            List<EntityReward> rewards = [];
            if (type.IsAssignableTo(typeof(Item)))
            {
                var createdType = (Item)Activator.CreateInstance(type)!;
                createdType.Stacks = amount;
                rewards.Add(new EntityReward([createdType]));
            }
            else
            {
                foreach (var _ in Enumerable.Range(0, amount))
                {
                    var createdType = (IInventoryEntity)Activator.CreateInstance(type)!;
                    if (createdType is Gear gear)
                        gear.Initialize(Rarity.FiveStar);

                    rewards.Add(new EntityReward([createdType]));
                }
            }


            var result = await userData
                .ReceiveRewardsAsync(DatabaseContext.Set<UserData>(), rewards);

            await DatabaseContext.SaveChangesAsync();
            var embed = new DiscordEmbedBuilder()
                .WithUser(userToGive)
                .WithColor(userData.Color)
                .WithTitle("Success!")
                .WithDescription(result);
            await ctx.RespondAsync(embed);
        }
    }
    [Command("give-me")]
    [Description("Use this to obtain anything that can be in inventory. Only available for izagawds use for testing")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public ValueTask Execute(CommandContext ctx, [Parameter("entity-name")] string entityName,
        [Parameter("entity-amount")] [Description("The amount you want of the supplied item")]
        int amount = 1)
    {
        return GiveSomeone(ctx, ctx.User, entityName, amount);
    }
}