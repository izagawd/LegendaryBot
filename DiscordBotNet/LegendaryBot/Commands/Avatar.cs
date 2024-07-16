using System.ComponentModel;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Avatar: GeneralCommandClass
{




    [Command("avatar")]
    [Description("Displays your avatar, or someone elses avatar")] 
    [AdditionalCommand("/avatar\n/avatar @user",BotCommandType.Fun)]
    public async ValueTask Execute(CommandContext ctx,  [Parameter("user")]
        [Description("if set, will display this users avatar instead")] 
        DiscordUser? user = null)
    {
        
        if(user is null)
        {
            user = ctx.User;
        }
        
        var color = await DatabaseContext.UserData
            .FindOrCreateSelectUserDataAsync(user.Id, i => i.Color);
        var embed = new DiscordEmbedBuilder()
            .WithTitle($"**{user.Username}'s avatar**")
            .WithAuthor(ctx.User.Username, iconUrl: ctx.User.AvatarUrl)
            .WithColor(color)
            .WithImageUrl(user.AvatarUrl)
            .WithTimestamp(DateTime.Now);
        await ctx.RespondAsync(embed);

    }
}