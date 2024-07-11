using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Info : GeneralCommandClass
{


 
    [SlashCommand("info", "Shows basic information about your progress, or someone else's progress"),
    AdditionalSlashCommand("/info\n/info @user",BotCommandType.Battle)]
    public async Task Execute(InteractionContext ctx,[Option("user","the user you want to check info about")]DiscordUser? author = null)
    {  

            
        if(author is null) author = ctx.User;
        
        var userData = await DatabaseContext.UserData.FindOrCreateUserDataAsync((long)author.Id);
  
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
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
        await ctx.CreateResponseAsync(response);




    }





}