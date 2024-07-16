using System.ComponentModel;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.Commands;

public class MakeMeDivine : GeneralCommandClass
{
    [Command("make-me-divine"), Description("Use this to give yourself highest tier"),
     AdditionalCommand("/make-me-divine", BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var userData = await DatabaseContext.UserData.FindOrCreateUserDataAsync(ctx.User.Id);
        if (userData.Tier == Tier.Divine)
        {
            await ctx.RespondAsync("you are already divine");
        }
        else
        {
            userData.Tier = Tier.Divine;
            await DatabaseContext.SaveChangesAsync();
            await ctx.RespondAsync("you are now divine!");
        }
     
        
    }
}