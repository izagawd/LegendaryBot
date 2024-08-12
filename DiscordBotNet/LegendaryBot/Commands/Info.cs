using System.ComponentModel;
using System.Reflection;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Info : GeneralCommandClass
{


 
    [Command("info"), Description("Use to see basic details about yourself or a player"),
    BotCommandCategory(BotCommandCategory.Other)]
    public async ValueTask Execute(CommandContext ctx,[Parameter("user")]DiscordUser? author = null)
    {  

            
        if(author is null) author = ctx.User;
        
        var userData = await DatabaseContext.UserData
            .AsNoTracking()
            .Include(i => i.Items.Where(j => j is Coin || j is DivineShard || j is Stamina))
            .FirstOrDefaultAsync(i => i.DiscordId == author.Id);

        if (userData is null || userData.Tier == Tier.Unranked)
        {
            if (author == ctx.User)
            {
                await AskToDoBeginAsync(ctx);
            }
            else
            {
                var embed = new DiscordEmbedBuilder()
                    .WithUser(author)
                    .WithTitle("Hmm")
                    .WithDescription($"{author.Username} has not begun with /begin")
                    .WithColor(TypesFunction.GetDefaultObject<UserData>().Color);
                await ctx.RespondAsync(embed);
            }
            return;
        }
        var stamina = userData.Items.GetOrCreateItem<Stamina>();
        stamina.RefreshEnergyValue();

        
        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Info")
            .WithUser(author)
            .WithColor(userData.Color)
            .AddField("Coins", $"`{userData.Items.GetItemStacks(typeof(Coin))}`", true)
            .AddField("Tier", $"`{userData.Tier}`", true)
            .AddField("Date Started", $"`{userData.StartTime}`", true)
            .AddField("Time Till Next Day", $"`{BasicFunctionality.TimeTillNextDay()}`", true)
            .AddField("Stamina", $"`{stamina.Stacks}`", true)
            .AddField("Divine Shards", $"`{userData.Items.GetItemStacks(typeof(DivineShard))}`")
            .WithImageUrl("attachment://info.png")
            .WithTimestamp(DateTime.Now);
        using var image = await userData.GetInfoAsync(author);
        await using var stream = new MemoryStream();
    
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embedBuilder)
            .AddFile("info.png", stream);
        await ctx.RespondAsync(response);
    }





}