
using System.ComponentModel;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;


namespace DiscordBotNet.LegendaryBot.command;
[Command("Quote")]
public class QuoteCommand : GeneralCommandClass
{

    [Command("read"), Description("Read a random quote"),
    AdditionalCommand("/read",BotCommandType.Fun)]
    public async Task Read(CommandContext ctx)
    {
        var anon = await DatabaseContext.Quote
            .Where(i => i.IsApproved)
            .Include(i => i.UserData)
            .Select(j =>
                new
                {
                    quote = j,
                    likes = j.QuoteReactions.Count(k => k.IsLike),
                    dislikes = j.QuoteReactions.Count(k => !k.IsLike)
                })
            .RandomOrDefaultAsync();
        if (anon is null)
        {
            await ctx.RespondAsync("damn");
            return;
        }

        var counts = new { anon.likes, anon.dislikes };
        var randomQuote = anon.quote;

        DiscordButtonComponent like = new(DiscordButtonStyle.Primary,"like",null,false,new DiscordComponentEmoji("👍"));
        DiscordButtonComponent dislike = new(DiscordButtonStyle.Primary, "dislike",null,false,new DiscordComponentEmoji("👎"));
        var ownerOfQuote = await ctx.Client.GetUserAsync( randomQuote.UserDataId);
        var quoteDate = randomQuote.DateCreated;
        
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ownerOfQuote)
            .WithColor(randomQuote.UserData.Color)
            .WithTitle($"{ownerOfQuote.Username}'s quote")
            .WithDescription(randomQuote.QuoteValue)
            .WithFooter($"Date and Time Created: {quoteDate:MM/dd/yyyy HH:mm:ss}\nLikes: {counts.likes} Dislikes: {counts.dislikes}");
        await ctx.RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder).AddComponents(like,dislike));
        var message = await ctx.GetResponseAsync();

        while (true)
        {
            var result = await message.WaitForButtonAsync(ctx.User);
            if(result.TimedOut) break;
            var interactivityResult = result.Result;
            var choice = interactivityResult.Interaction.Data.CustomId;
            if (!new[] { "like", "dislike" }.Contains(choice)) return;
            await using var newDbContext = new PostgreSqlContext();
            var anonymous = await newDbContext.Set<QuoteReaction>()
                .Where(j => j.QuoteId == randomQuote.Id && j.UserDataId == interactivityResult.User.Id)
                .Select(j => new
                {
                    quote = j.Quote, quoteReaction = j,
                })
                .FirstOrDefaultAsync();
            var quoteReaction = anonymous?.quoteReaction;
            if (anonymous?.quote is not null)
            {
                randomQuote = anonymous.quote;
            }
            var isNew = false;
            if (quoteReaction is null)
            {
                quoteReaction = new QuoteReaction();
                await newDbContext.Set<QuoteReaction>().AddAsync(quoteReaction);
                //assigning it to id instead of instance cuz instance might not be of the same dbcontext as newDbContext
                quoteReaction.QuoteId = randomQuote.Id;
                quoteReaction.UserDataId = interactivityResult.User.Id;
                isNew = true;
            }

            if (!await newDbContext.UserData.AnyAsync(j => j.Id == interactivityResult.User.Id))
                await newDbContext.UserData.AddAsync(new UserData(interactivityResult.User.Id));
            if (!isNew &&
                ((choice == "like" && quoteReaction.IsLike) || (choice == "dislike" && !quoteReaction.IsLike)))
            {
                newDbContext.Set<QuoteReaction>().Remove(quoteReaction);

            }
            else if (choice == "like")
            {
                quoteReaction.IsLike = true;
            }
            else
            {
                quoteReaction.IsLike = false;
            }

            await newDbContext.SaveChangesAsync();
            var localCounts = await newDbContext.Quote.Where(i => i.Id == randomQuote.Id)
                .Select(i => new
                {
                    likes = i.QuoteReactions.Count(j => j.IsLike),
                    dislikes = i.QuoteReactions.Count(j => !j.IsLike)
                }).FirstAsync();

            embedBuilder
                .WithFooter(
                    $"Date and Time Created: {quoteDate:MM/dd/yyyy HH:mm:ss}\nLikes: {localCounts.likes} Dislikes: {localCounts.dislikes}");

            await interactivityResult.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()

                    .AddEmbed(embedBuilder)
                    .AddComponents(like, dislike));
          


        }

    }
    [Command("write"), Description("write a quote for everyone to see!"),
    AdditionalCommand("/write Be good and good will come",BotCommandType.Fun)]
    public async Task Write(CommandContext ctx, [Parameter("text")] string text)
    {
        var userData = await DatabaseContext.UserData.FindOrCreateUserDataAsync(ctx.User.Id);
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(userData.Color);
        if (text.Length > 200)
        {
            embedBuilder
                .WithTitle("Hmm")
                .WithDescription("Quote length should be shorter than 200 characters");

            await ctx.RespondAsync(embedBuilder);
            return;
        } 
        if (text.Length <= 0)
        {
            embedBuilder
                .WithTitle("bruh")
                .WithDescription("Write something in the quote!");
            await ctx.RespondAsync(embedBuilder);
            return;
        }
        userData.Quotes.Add(new LegendaryBot.Quote{QuoteValue = text});
        await DatabaseContext.SaveChangesAsync();
        embedBuilder
            .WithTitle("Success!")
            .WithDescription("Your quote has been saved, and is waiting to be approved!")
            .AddField("Your quote was: ", text);
        await ctx.RespondAsync(embedBuilder);
    }
}