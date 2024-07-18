using System.ComponentModel;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Daily : GeneralCommandClass
{

    [Command("daily"), Description("Use this everyday to collect daily rewards"),
    AdditionalCommand("/daily",BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var userData =await  DatabaseContext.UserData.FirstOrDefaultAsync(i => i.Id == ctx.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }
        var embed = new DiscordEmbedBuilder()
            .WithColor(userData.Color)
            .WithUser(ctx.User)
            .WithTitle("Hmm");
        if (userData.Tier == Tier.Unranked)
        {
            await ctx.RespondAsync(embed.WithDescription("Use the begin Commands first"));
            return;
        }

        if (!userData.DailyPending)
        {
            await ctx.RespondAsync(embed.WithDescription("You have already collected your dailies"));
            return;
        }

        userData.ShardsOfTheGods += 100;
        userData.LastTimeDailyWasChecked = DateTime.UtcNow;
        userData.Coins += 10000;
        await DatabaseContext.SaveChangesAsync();
        await ctx.RespondAsync(embed.WithTitle("Nice!!")
            .WithDescription($"You gained 100 {nameof(userData.ShardsOfTheGods).Englishify()} and 10,000 coins!"));
    }
}