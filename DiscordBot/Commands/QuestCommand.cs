using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Quests;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class QuestCommand : GeneralCommandClass
{
    [Command("quest")] [Description("Use this command to do daily quests!")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var author = ctx.User;

        await DatabaseContext.CheckForNewDayAsync(author.Id);
        await DatabaseContext.SaveChangesAsync();
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Quests)
            .FirstOrDefaultAsync(i => i.DiscordId == author.Id);
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

        var questString = "";
        var embed = new DiscordEmbedBuilder()
            .WithUser(author)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription("you have no quests");


        if (userData.Quests.Count <= 0)
        {
            await ctx.RespondAsync(embed);
            return;
        }

        await MakeOccupiedAsync(userData);
        var count = 1;
        foreach (var i in userData.Quests)
        {
            var preparedString = $"{count}. {i.Title}: {i.Description}.";
            if (i.Completed)
                preparedString += " (Completed)";
            questString += $"{preparedString}\n";
            count++;
        }

        var questsShouldDisable = userData.Quests.Select(i => i.Completed).ToList();
        while (questsShouldDisable.Count < 4)
            questsShouldDisable.Add(true);
        var one = new DiscordButtonComponent(DiscordButtonStyle.Primary, "1", "1",
            questsShouldDisable[0]);
        var two = new DiscordButtonComponent(DiscordButtonStyle.Primary, "2", "2", questsShouldDisable[1]);
        var three = new DiscordButtonComponent(DiscordButtonStyle.Primary, "3", "3", questsShouldDisable[2]);
        var four = new DiscordButtonComponent(DiscordButtonStyle.Primary, "4", "4", questsShouldDisable[3]);
        var cancel = new DiscordButtonComponent(DiscordButtonStyle.Danger, "cancel", "CANCEL");
        embed
            .WithTitle("These are your available quests. They refresh at midnight UTC")
            .WithDescription(questString);
        DiscordComponent[] comps = [one, two, three, four, cancel];
        await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
            .AddComponents(comps)
            .AddEmbed(embed));
        IEnumerable<string> possibleCustomIds = ["1", "2", "3", "4"];
        var message = (await ctx.GetResponseAsync())!;

       
        var buttonResult = await message.WaitForButtonAsync(ctx.User);
        
        if(buttonResult.TimedOut)
            return;
        if (buttonResult.Result.Id == "cancel")
        {
            foreach (var i in comps.OfType<DiscordButtonComponent>())
            {
                i.Disable();
            }
            await buttonResult.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AddComponents(comps));
            return;
        }

        var clickedId = buttonResult.Result.Id;
        var quest = userData.Quests[int.Parse(clickedId) - 1];

        await buttonResult.Result
            .Interaction
            .CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
        var succeeded = await quest.StartQuest(DatabaseContext.Set<UserData>(), ctx, message);

        if (succeeded.IsSuccess)
        {
            quest.Completed = true;
            var expToAdd = 1000;
            
            var rewardString =await userData.ReceiveRewardsAsync(DatabaseContext.Set<UserData>(), [
                ..(succeeded.Rewards ?? []), new UserExperienceReward(expToAdd),
                new EntityReward([new DivineShard { Stacks = 25 }])
            ]);

            embed
                .WithTitle("Nice!!")
                .WithDescription("**You completed the quest!**\n\n" + rewardString);
            await DatabaseContext.SaveChangesAsync();
            await message.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
            return;
        }

        embed
            .WithTitle("Damn")
            .WithDescription("You failed");
        await message.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
    }
}