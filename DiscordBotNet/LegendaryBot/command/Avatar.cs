using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;


namespace DiscordBotNet.LegendaryBot.command;

public class Avatar: GeneralCommandClass
{




    [SlashCommand("avatar", "Displays your avatar, or someone elses avatar"),
    AdditionalSlashCommand("/avatar\n/avatar @user",BotCommandType.Fun)]
    public async Task Execute(InteractionContext ctx, [Option("user", "User to display avatar. if none selected, self avatar will be displayed")] DiscordUser? user = null)
    {
        if(user is null)
        {
            user = ctx.User;
        }
        
        DiscordColor color = await DatabaseContext.UserData
            .FindOrCreateSelectUserDataAsync((long)user.Id, i => i.Color);
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle($"**{user.Username}'s avatar**")
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithColor(color)
            .WithImageUrl(user.AvatarUrl)
            .WithTimestamp(DateTime.Now);
        await ctx.CreateResponseAsync(embed);

    }
}