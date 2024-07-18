using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Info : GeneralCommandClass
{


 
    [Command("info"),
    AdditionalCommand("/info\n/info @user",BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx,[Parameter("user")]DiscordUser? author = null)
    {  

            
        if(author is null) author = ctx.User;
        
        var userData = await DatabaseContext.UserData.FirstOrDefaultAsync(i => i.Id == ctx.User.Id);

        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }
        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Info")
            .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
            .WithColor(userData.Color)
            .AddField("Coins", $"`{userData.Coins}`", true)
            .AddField("Experience", $"`{userData.Experience}`", true)
            .AddField("Tier", $"`{userData.Tier}`", true)
            .AddField("Date Started", $"`{userData.StartTime}`", true)
            .AddField("Time Till Next Day", $"`{BasicFunctionality.TimeTillNextDay()}`", true)
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