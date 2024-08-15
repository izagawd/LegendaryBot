using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Quests;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Functionality;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class QuestCommand : GeneralCommandClass
{


    [Command("quest"),
    BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var author = ctx.User;

        await DatabaseContext.CheckForNewDayAsync(author.Id);
        await DatabaseContext.SaveChangesAsync();
        var userData = await DatabaseContext.UserData
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
        while(questsShouldDisable.Count < 4)
            questsShouldDisable.Add(true);
        var one = new DiscordButtonComponent(DiscordButtonStyle.Primary, "1", "1",
            questsShouldDisable[0]);
        var two = new DiscordButtonComponent(DiscordButtonStyle.Primary, "2", "2",questsShouldDisable[1]);
        var three = new DiscordButtonComponent(DiscordButtonStyle.Primary, "3", "3",questsShouldDisable[2]);
        var four = new DiscordButtonComponent(DiscordButtonStyle.Primary, "4", "4",questsShouldDisable[3]);
        embed
            .WithTitle("These are your available quests. They refresh at midnight UTC")
            .WithDescription(questString);
        await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
            .AddComponents([one, two, three, four])
            .AddEmbed(embed));
        IEnumerable<string> possibleCustomIds = ["1", "2", "3", "4"];
        var message =await ctx.GetResponseAsync();
        
        Quest quest = null;
        var buttonResult = await message.WaitForButtonAsync(i =>
        {
            if (i.User.Id != ctx.User.Id) return false;
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
            var expToAdd = 1000;

            await DatabaseContext.Items.Where(i => i.UserDataId == userData.Id && i is DivineShard)
                .LoadAsync();
            var rewardString = userData.ReceiveRewards([..quest.QuestRewards,new UserExperienceReward(expToAdd),
            new EntityReward([new DivineShard(){Stacks = 10}])]);

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