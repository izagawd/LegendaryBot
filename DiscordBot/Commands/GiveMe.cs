using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class GiveMe : GeneralCommandClass
{
    [Command("give-me")]
    [Description("Use this to obtain anything that can be in inventory. Only available for izagawds use for testing")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask Execute(CommandContext ctx, [Parameter("entity-name")] string entityName,
        [Parameter("entity-amount")] [Description("The amount you want of the supplied item")]
        int amount = 1)
    {
        var simplifiedEntityName = entityName.ToLower().Replace(" ", "");
        var type = TypesFunction
            .GetDefaultObjectsAndSubclasses<IInventoryEntity>()
            .FirstOrDefault(i => i.Name.ToLower().Replace(" ", "") == simplifiedEntityName)?.GetType();
        if (ctx.User.Id != DiscordBot.Izasid && ctx.User.Id != DiscordBot.Testersid)
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
                .FirstOrDefaultAsync(i => i.DiscordId == ctx.User.Id);
            if (userData is null || userData.Tier == Tier.Unranked)
            {
                await AskToDoBeginAsync(ctx);
                return;
            }

            if (userData.IsOccupied)
            {
                await NotifyAboutOccupiedAsync(ctx);
                return;
            }

            List<EntityReward> rewards = [];
            foreach (var i in Enumerable.Range(0, amount))
            {
                var createdType = (IInventoryEntity)Activator.CreateInstance(type)!;
                if (createdType is Item item)
                    item.Stacks = 1;
                if (createdType is Gear gear)
                    gear.Initialize(Rarity.FiveStar);

                rewards.Add(new EntityReward([createdType]));
            }

            var result = await userData.ReceiveRewardsAsync(DatabaseContext.Set<UserData>(), rewards);

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