using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class Color : GeneralCommandClass
{
    [Command("color")]
    [Description("Use this to change your color")]
    [BotCommandCategory]
    public async ValueTask Execute(CommandContext ctx,
        [SlashChoiceProvider<ColorChoice>] [Parameter("name")]
        string colorName)
    {
        var author = ctx.User;

        var userData = await DatabaseContext.Set<UserData>()
            .Where(i => i.DiscordId == ctx.User.Id)
            .Select(i => new { i.Color, i.IsOccupied })
            .FirstOrDefaultAsync();

        if (userData is null)
        {
            var created = await DatabaseContext.CreateNonExistantUserdataAsync(author.Id);
            userData = new { created.Color, created.IsOccupied };
            await DatabaseContext.SaveChangesAsync();
        }

        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(ctx);
            return;
        }

        var color = userData.Color;
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
            .WithColor(color)
            .WithTimestamp(DateTime.Now);
        if (colorIsValid)
        {
            await DatabaseContext.Set<UserData>().ExecuteUpdateAsync(i => i
                .SetProperty(j => j.Color, color));
            embed.WithTitle("**Success!**");
            embed.WithDescription("`Look at your new color!`");
        }
        else
        {
            embed.WithColor(color);
            embed.WithTitle("**Hmm**");
            embed.WithDescription("`That color is not available`");
        }

        await ctx.RespondAsync(embed.Build());
    }

    private class ColorChoice : IChoiceProvider
    {
        private static readonly IReadOnlyDictionary<string, object> _choices = new Dictionary<string, object>
        {
            ["Blue"] = "blue",
            ["Red"] = "red",
            ["Green"] = "green",
            ["Orange"] = "orange",
            ["Purple"] = "purple"
        };

        public ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter)
        {
            return ValueTask.FromResult(_choices);
        }
    }
}