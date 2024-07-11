
using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;


namespace DiscordBotNet.LegendaryBot.command;

public class Color : GeneralCommandClass
{


    [SlashCommand("color", "Changes the color of the embed messages I send to you"),
    AdditionalSlashCommand("/color Blue",BotCommandType.Other)]
    public async Task Execute(InteractionContext ctx,
        [Choice("Blue","blue")]
        [Choice("Red","red")]
        [Choice("Green","green")]
        [Choice("Orange","orange")]
        [Choice("Purple","purple")]
        [Choice("Green","green")]
        [Option("name","The name of the color you want to change to")] string colorName)
    {
        DiscordUser author = ctx.User;

        var userData = await DatabaseContext.UserData.FindOrCreateUserDataAsync((long)author.Id);
  

        
        bool colorIsValid = true;
        DiscordColor newColor;
        switch (colorName.ToLower())
        {
            case "blue":
                newColor = DiscordColor.Blue;
                break;
            case "red":
                newColor = DiscordColor.Red;
                break;
            case "green":
                newColor = DiscordColor.Green;
                break;
            case "orange":
                newColor = DiscordColor.Orange;
                break;
            case "purple":
                newColor = DiscordColor.Purple;
                break;
            default:
                newColor = userData.Color;
                colorIsValid = false;
                break;

        }
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
            .WithColor(newColor)
            .WithTimestamp(DateTime.Now);
        if (colorIsValid)
        {
            userData.Color = newColor;
            await DatabaseContext.SaveChangesAsync();
            embed.WithTitle("**Success!**");
            embed.WithDescription("`Look at your new color!`");
        }
        else
        {
            embed.WithColor(userData.Color);
            embed.WithTitle("**Hmm**");
            embed.WithDescription("`That color is not available`");
        }
        await ctx.CreateResponseAsync(embed: embed.Build());
          
    }
       

}