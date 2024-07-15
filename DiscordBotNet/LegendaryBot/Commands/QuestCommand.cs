using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Quests;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public class QuestCommand : GeneralCommandClass
{


    [Command("quest"),
    AdditionalCommand("/quest",BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var author = ctx.User;

        await DatabaseContext.CheckForNewDayAsync(author.Id);
        await DatabaseContext.SaveChangesAsync();
        var userData = await DatabaseContext.UserData
            .Include(i => i.Quests)
            .FindOrCreateUserDataAsync(author.Id);
        var questString = "";
        var embed = new DiscordEmbedBuilder()
            .WithUser(author)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription("you have no quests");
        if (userData.Tier == Tier.Unranked)
        {
            
            embed.WithDescription("Use the begin Commands before doing any quests");
            await ctx.RespondAsync(embed);
            return;
        }

        if (userData.Quests.Count <= 0)
        {
           await ctx.RespondAsync(embed);
           return;
        }

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
        while(questsShouldDisable.Count < 4)
            questsShouldDisable.Add(true);
        var one = new DiscordButtonComponent(DiscordButtonStyle.Primary, "1", "1",
            questsShouldDisable[0]);
        var two = new DiscordButtonComponent(DiscordButtonStyle.Primary, "2", "2",questsShouldDisable[1]);
        var three = new DiscordButtonComponent(DiscordButtonStyle.Primary, "3", "3",questsShouldDisable[2]);
        var four = new DiscordButtonComponent(DiscordButtonStyle.Primary, "4", "4",questsShouldDisable[3]);
        embed
            .WithTitle("These are your available quests")
            .WithDescription(questString);
        await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
            .AddComponents([one, two, three, four])
            .AddEmbed(embed));
        IEnumerable<string> possibleCustomIds = ["1", "2", "3", "4"];
        var message =await ctx.GetResponseAsync();
        
        Quest quest = null;
        var buttonResult = await message.WaitForButtonAsync(i =>
        {
            if (i.User.Id != (ulong)userData.Id) return false;
            if (!possibleCustomIds.Contains(i.Id)) return false;
            quest = userData.Quests[int.Parse(i.Id) -1];
            return true;
        });
        if (quest is null)
        {
            return;
        }

        await buttonResult.Result
            .Interaction
            .CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
        var succeeded = await quest.StartQuest(DatabaseContext, ctx, message);
        
        if (succeeded)
        {
            quest.Completed = true;
            var expToAdd = 40l;
            switch (userData.Tier)
            {
                case Tier.Bronze:
                    expToAdd = 40;
                    break;
                case Tier.Silver:
                    expToAdd = 800;
                    break;
                case Tier.Gold:
                    expToAdd = 1600;
                    break;
                case Tier.Platinum:
                    expToAdd = 3200;
                    break;
                case Tier.Diamond:
                    expToAdd = 6400;
                    break;
                case Tier.Divine:
                    expToAdd = 12800;
                    break;
                }
            var rewards = quest.QuestRewards.Append(new UserExperienceReward(expToAdd));
            var rewardString = userData.ReceiveRewards(ctx.User.Username, rewards);
            embed
                .WithTitle("Nice!!")
                .WithDescription("You completed the quest!\n" +rewardString);
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