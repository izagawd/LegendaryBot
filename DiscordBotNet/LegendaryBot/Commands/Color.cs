using System.ComponentModel;
using DiscordBotNet.Database.Models;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Color : GeneralCommandClass
{

    private class ColorChoice : IChoiceProvider
    {
        private static readonly IReadOnlyDictionary<string, object> _choices = new Dictionary<string, object>
        {
            ["Blue"] = "blue",
            ["Red"] = "red",
            ["Green"] = "green",
            ["Orange"] = "orange",
            ["Purple"] = "purple",
        };

        public ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter) => 
            ValueTask.FromResult(_choices);
    }
    [Command("color"),Description("Use this to change your color"),
    BotCommandCategory(BotCommandCategory.Other)]
    public async ValueTask Execute(CommandContext ctx,
        [SlashChoiceProvider<ColorChoice>]
        
        [Parameter("name")] string colorName)
    {
        var author = ctx.User;

        var userData = await DatabaseContext.UserData
            .Where(i => i.Id == ctx.User.Id)
            .Select(i => new{i.Color})
            .FirstOrDefaultAsync();

        if (userData is null)
        {
            await DatabaseContext.CreateNonExistantUserdataAsync(author.Id);
            await DatabaseContext.SaveChangesAsync();
            userData = new { TypesFunctionality.GetDefaultObject<UserData>().Color };
        }
        
        var colorIsValid = true;
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
        var embed = new DiscordEmbedBuilder()
            .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
            .WithColor(newColor)
            .WithTimestamp(DateTime.Now);
        if (colorIsValid)
        {
            await DatabaseContext.UserData
                .Where(i => i.Id == ctx.User.Id)
                .ExecuteUpdateAsync(i => i.SetProperty(j => j.Color,
                    newColor));
            embed.WithTitle("**Success!**");
            embed.WithDescription("`Look at your new color!`");
        }
        else
        {
            embed.WithColor(userData.Color);
            embed.WithTitle("**Hmm**");
            embed.WithDescription("`That color is not available`");
        }
        await ctx.RespondAsync(embed: embed.Build());
          
    }
       

}