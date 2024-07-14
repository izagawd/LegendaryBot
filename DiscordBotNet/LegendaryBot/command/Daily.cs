using System.ComponentModel;
using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.command;

public class Daily : GeneralCommandClass
{

    [Command("daily"), Description("Use this everyday to collect daily rewards"),
    AdditionalCommand("/daily",BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var userData =await  DatabaseContext.UserData.FindOrCreateUserDataAsync(ctx.User.Id);
        var embed = new DiscordEmbedBuilder()
            .WithColor(userData.Color)
            .WithUser(ctx.User)
            .WithTitle("Hmm");
        if (userData.Tier == Tier.Unranked)
        {
            await ctx.RespondAsync(embed.WithDescription("Use the begin command first"));
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