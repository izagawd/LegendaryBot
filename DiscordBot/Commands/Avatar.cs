using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class Avatar : GeneralCommandClass
{
    [Command("avatar")]
    [Description("Displays your avatar, or someone elses avatar")]
    [BotCommandCategory]
    public async ValueTask Execute(CommandContext ctx,
        [Parameter("user")] [Description("if set, will display this users avatar instead")]
        DiscordUser? user = null)
    {
        if (user is null) user = ctx.User;


        var color = await DatabaseContext.UserData
            .Where(i => i.DiscordId == user.Id)
            .Select(i => new DiscordColor?(i.Color))
            .FirstOrDefaultAsync();
        if (color is null) color = TypesFunction.GetDefaultObject<UserData>().Color;
        var embed = new DiscordEmbedBuilder()
            .WithTitle($"**{user.Username}'s avatar**")
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithColor(color.Value)
            .WithImageUrl(user.AvatarUrl)
            .WithTimestamp(DateTime.Now);
        await ctx.RespondAsync(embed);
    }
}