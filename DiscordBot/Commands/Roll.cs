using System.ComponentModel;
using BattleManagemen.LegendaryBot;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class Roll : GeneralCommandClass
{
    [Command("roll")]
    [Description("roll a random number form 0 - 100!")]
    [BotCommandCategory]
    public async ValueTask Execute(CommandContext ctx)
    {
        var color = await DatabaseContext.UserData
                        .Where(i => i.DiscordId == ctx.User.Id)
                        .Select(i => new DiscordColor?(i.Color))
                        .FirstOrDefaultAsync()
                    ?? TypesFunction.GetDefaultObject<UserData>().Color;
        var random = new Random();
        var embed = new DiscordEmbedBuilder()
            .WithTitle("**Roll**")
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithColor(color)
            .WithDescription($"{random.Next(0, 100)}:game_die:");
        await ctx.RespondAsync(embed.Build());
    }
}