
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;


namespace DiscordBotNet.LegendaryBot.command;
[SlashCommandGroup("Quote", "Post a quote or send a quote")]
public class QuoteCommand : GeneralCommandClass
{

    [SlashCommand("read", "gets a random quote"),
    AdditionalSlashCommand("/read",BotCommandType.Fun)]
    public async Task Read(InteractionContext ctx)
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
            await ctx.CreateResponseAsync("damn");
            return;
        }

        var counts = new { anon.likes, anon.dislikes };
        var randomQuote = anon.quote;

        DiscordButtonComponent like = new(ButtonStyle.Primary,"like",null,false,new DiscordComponentEmoji("👍"));
        DiscordButtonComponent dislike = new(ButtonStyle.Primary, "dislike",null,false,new DiscordComponentEmoji("👎"));
        var ownerOfQuote = await ctx.Client.GetUserAsync((ulong) randomQuote.UserDataId);
        var quoteDate = randomQuote.DateCreated;
        
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ownerOfQuote)
            .WithColor(randomQuote.UserData.Color)
            .WithTitle($"{ownerOfQuote.Username}'s quote")
            .WithDescription(randomQuote.QuoteValue)
            .WithFooter($"Date and Time Created: {quoteDate:MM/dd/yyyy HH:mm:ss}\nLikes: {counts.likes} Dislikes: {counts.dislikes}");
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder).AddComponents(like,dislike));
        var message = await ctx.GetOriginalResponseAsync();

        while (true)
        {
            var result = await message.WaitForButtonAsync(ctx.User);
            if(result.TimedOut) break;
            var interactivityResult = result.Result;
            var choice = interactivityResult.Interaction.Data.CustomId;
            if (!new[] { "like", "dislike" }.Contains(choice)) return;
            await using var newDbContext = new PostgreSqlContext();
            var anonymous = await newDbContext.Set<QuoteReaction>()
                .Where(j => j.QuoteId == randomQuote.Id && j.UserDataId ==(long) interactivityResult.User.Id)
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
                quoteReaction.UserDataId = (long)interactivityResult.User.Id;
                isNew = true;
            }

            if (!await newDbContext.UserData.AnyAsync(j => j.Id ==(long) interactivityResult.User.Id))
                await newDbContext.UserData.AddAsync(new UserData((long)interactivityResult.User.Id));
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

            await interactivityResult.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()

                    .AddEmbed(embedBuilder)
                    .AddComponents(like, dislike));
          


        }

    }
    [SlashCommand("write", "creates a quote"),
    AdditionalSlashCommand("/write Be good and good will come",BotCommandType.Fun)]
    public async Task Write(InteractionContext ctx, [Option("text", "the quote")] string text)
    {
        var userData = await DatabaseContext.UserData.FindOrCreateUserDataAsync((long)ctx.User.Id);
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(userData.Color);
        if (text.Length > 200)
        {
            embedBuilder
                .WithTitle("Hmm")
                .WithDescription("Quote length should be shorter than 200 characters");

            await ctx.CreateResponseAsync(embedBuilder);
            return;
        } 
        if (text.Length <= 0)
        {
            embedBuilder
                .WithTitle("bruh")
                .WithDescription("Write something in the quote!");
            await ctx.CreateResponseAsync(embedBuilder);
            return;
        }
        userData.Quotes.Add(new LegendaryBot.Quote{QuoteValue = text});
        await DatabaseContext.SaveChangesAsync();
        embedBuilder
            .WithTitle("Success!")
            .WithDescription("Your quote has been saved, and is waiting to be approved!")
            .AddField("Your quote was: ", text);
        await ctx.CreateResponseAsync(embedBuilder);
    }
}