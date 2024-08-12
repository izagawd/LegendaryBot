using System.ComponentModel;
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

        var color = await DatabaseContext.UserData
      
            .Where(i =>  i.DiscordId == ctx.User.Id)
            .Select(i => new DiscordColor?(i.Color))
            .FirstOrDefaultAsync();

        if (color is null)
        {
            color = (await DatabaseContext.CreateNonExistantUserdataAsync(author.Id)).Color;
            await DatabaseContext.SaveChangesAsync();
        }
        
        var colorIsValid = true;
        switch (colorName.ToLower())
        {
            case "blue":
                color = DiscordColor.Blue;
                break;
            case "red":
                color = DiscordColor.Red;
                break;
            case "green":
                color = DiscordColor.Green;
                break;
            case "orange":
                color = DiscordColor.Orange;
                break;
            case "purple":
                color = DiscordColor.Purple;
                break;
        }
        var embed = new DiscordEmbedBuilder()
            .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
            .WithColor(color.Value)
            .WithTimestamp(DateTime.Now);
        if (colorIsValid)
        {
            await DatabaseContext.UserData.ExecuteUpdateAsync(i => i
                .SetProperty(j => j.Color, color.Value));
            embed.WithTitle("**Success!**");
            embed.WithDescription("`Look at your new color!`");
        }
        else
        {
            embed.WithColor(color.Value);
            embed.WithTitle("**Hmm**");
            embed.WithDescription("`That color is not available`");
        }
        await ctx.RespondAsync(embed: embed.Build());
          
    }
       

}