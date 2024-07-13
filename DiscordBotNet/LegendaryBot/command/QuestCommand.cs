using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Quests;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public class QuestCommand : GeneralCommandClass
{


    [SlashCommand("quest", "do a daily quest"),
    AdditionalSlashCommand("/quest",BotCommandType.Battle)]
    public async Task Execute(InteractionContext ctx)
    {
        var author = ctx.User;

        await DatabaseContext.CheckForNewDayAsync((long)author.Id);
        await DatabaseContext.SaveChangesAsync();
        var userData = await DatabaseContext.UserData
            .Include(i => i.Quests)
            .FindOrCreateUserDataAsync((long)author.Id);
        var questString = "";
        var embed = new DiscordEmbedBuilder()
            .WithUser(author)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription("you have no quests");
        if (userData.Tier == Tier.Unranked)
        {
            
            embed.WithDescription("Use the begin command before doing any quests");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        if (userData.Quests.Count <= 0)
        {
           await ctx.CreateResponseAsync(embed);
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
        DiscordButtonComponent one = new DiscordButtonComponent(ButtonStyle.Primary, "1", "1",
            questsShouldDisable[0]);
        DiscordButtonComponent two = new DiscordButtonComponent(ButtonStyle.Primary, "2", "2",questsShouldDisable[1]);
        DiscordButtonComponent three = new DiscordButtonComponent(ButtonStyle.Primary, "3", "3",questsShouldDisable[2]);
        DiscordButtonComponent four = new DiscordButtonComponent(ButtonStyle.Primary, "4", "4",questsShouldDisable[3]);
        embed
            .WithTitle("These are your available quests")
            .WithDescription(questString);
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
            .AddComponents([one, two, three, four])
            .AddEmbed(embed));
        IEnumerable<string> possibleCustomIds = ["1", "2", "3", "4"];
        var message =await ctx.GetOriginalResponseAsync();
        
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
            .CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        var succeeded = await quest.StartQuest(DatabaseContext, ctx, message);
        
        if (succeeded)
        {
            quest.Completed = true;
            var expToAdd = 40ul;
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
            var rewards = quest.QuestRewards.Append(new UserExperienceReward((long)expToAdd));
            var rewardString = userData.ReceiveRewards(ctx.User.Username, rewards);
            embed
                .WithTitle("Nice!!")
                .WithDescription("You completed the quest!\n" +rewardString);
            await DatabaseContext.SaveChangesAsync();
            await message.ModifyAsync(new DiscordMessageBuilder() { Embed = embed });
            return;
        }
        embed
            .WithTitle("Damn")
            .WithDescription("You failed");
        await message.ModifyAsync(new DiscordMessageBuilder() { Embed = embed });
       
    }
}