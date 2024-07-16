using System.ComponentModel;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Roll :  GeneralCommandClass
{

    [Command("roll"), Description("roll a random number form 0 - 100!"),
    AdditionalCommand("/roll",BotCommandType.Fun)]
    public async ValueTask Execute(CommandContext ctx)
    {
        
        var color = await DatabaseContext.UserData
            .FindOrCreateSelectUserDataAsync(ctx.User.Id, i => i.Color);
        var random = new Random();
        var embed = new DiscordEmbedBuilder()
            .WithTitle("**Roll**")
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithColor(color)
            .WithDescription($"{random.Next(0, 100)}:game_die:");
        await ctx.RespondAsync(embed.Build());
    }


}