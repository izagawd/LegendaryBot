using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Daily : GeneralCommandClass
{

    [SlashCommand("daily", "Gets daily rewards"),
    AdditionalSlashCommand("/daily",BotCommandType.Battle)]
    public async Task Execute(InteractionContext ctx)
    {
        var userData =await  DatabaseContext.UserData.FindOrCreateUserDataAsync((long)ctx.User.Id);
        var embed = new DiscordEmbedBuilder()
            .WithColor(userData.Color)
            .WithUser(ctx.User)
            .WithTitle("Hmm");
        if (userData.Tier == Tier.Unranked)
        {
            await ctx.CreateResponseAsync(embed.WithDescription("Use the begin command first"));
            return;
        }

        if (!userData.DailyPending)
        {
            await ctx.CreateResponseAsync(embed.WithDescription("You have already collected your dailies"));
            return;
        }

        userData.ShardsOfTheGods += 100;
        userData.LastTimeDailyWasChecked = DateTime.UtcNow;
        userData.Coins += 10000;
        await DatabaseContext.SaveChangesAsync();
        await ctx.CreateResponseAsync(embed.WithTitle("Nice!!")
            .WithDescription($"You gained 100 {nameof(userData.ShardsOfTheGods).Englishify()} and 10,000 coins!"));
    }
}