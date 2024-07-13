using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class MakeMeDivine : GeneralCommandClass
{
    [SlashCommand("make_me_divine", "makes you the highest tier, which is divine"),
     AdditionalSlashCommand("/make_me_divine", BotCommandType.Battle)]
    public async Task Execute(InteractionContext ctx)
    {
        var userData = await DatabaseContext.UserData.FindOrCreateUserDataAsync((long) ctx.User.Id);
        if (userData.Tier == Tier.Divine)
        {
            await ctx.CreateResponseAsync("you are already divine");
        }
        else
        {
            userData.Tier = Tier.Divine;
            await DatabaseContext.SaveChangesAsync();
            await ctx.CreateResponseAsync("you are now divine!");
        }
     
        
    }
}