using System.ComponentModel;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Roll :  GeneralCommandClass
{

    [Command("roll"), Description("roll a random number form 0 - 100!"),
    AdditionalCommand("/roll",BotCommandType.Fun)]
    public async ValueTask Execute(CommandContext ctx)
    {

        var color = await DatabaseContext.UserData
            .Where(i => i.Id == ctx.User.Id)
            .Select(i => i.Color)
            .FirstOrDefaultAsync();
        var random = new Random();
        var embed = new DiscordEmbedBuilder()
            .WithTitle("**Roll**")
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithColor(color)
            .WithDescription($"{random.Next(0, 100)}:game_die:");
        await ctx.RespondAsync(embed.Build());
    }


}