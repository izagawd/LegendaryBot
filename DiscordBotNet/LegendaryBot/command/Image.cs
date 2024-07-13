using DSharpPlus.Entities;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.command;

public class Image : GeneralCommandClass
{
    [Command("image")]
    public async ValueTask Execute(CommandContext ctx)
    {
     
        await ctx.RespondAsync(new DiscordEmbedBuilder()
            .WithImageUrl("https://miro.medium.com/v2/resize:fit:721/1*9m0eBRsmz2-_in8K9_D_gA.png")
            .WithTitle("bruh")
            .WithDescription("bruh"));
    }

}