using System.ComponentModel;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class MakeMeDivine : GeneralCommandClass
{
    [Command("make-me-divine"), Description("Use this to give yourself highest tier"),
     AdditionalCommand("/make-me-divine", BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var userData = await DatabaseContext.UserData
            .FirstOrDefaultAsync(i => i.Id == ctx.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }
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