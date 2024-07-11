using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Roll :  GeneralCommandClass
{

    [SlashCommand("roll", "Gets a random number"),
    AdditionalSlashCommand("/roll",BotCommandType.Fun)]
    public async Task Execute(InteractionContext ctx)
    {
        
        DiscordColor color = await DatabaseContext.UserData
            .FindOrCreateSelectUserDataAsync((long)ctx.User.Id, i => i.Color);
        var random = new Random();
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("**Roll**")
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithColor(color)
            .WithDescription($"{random.Next(0, 100)}:game_die:");
        await ctx.CreateResponseAsync(embed.Build());
    }


}