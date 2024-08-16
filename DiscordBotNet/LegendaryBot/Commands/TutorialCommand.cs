using System.ComponentModel;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class TutorialCommand : GeneralCommandClass
{
    [Command("tutorial")]
    [Description("Guide on how to use stuff")]
    [BotCommandCategory()]
    public async Task Execute(CommandContext ctx)
    {
        const string tutorialString =
            "To start using bot, use `/begin`.\nto see which teams you have, use `/display teams`" +
            "\nTo change equipped team, use `/team equip-team` use `/help team` to see more team commands" +
            "\nTo battle a friend, use `/challenge @user`." +
            "\nIf you want to see what things you can get in this game, use `/display all-entities`" +
            "\n to use energy to get characters, use `/explore`" +
            "\nYou can do daily quests with `/quest`" +
            "\nNote: a blessing is like a weapon from genshin, or you could say a lightcone from honkai star rail";
        var embed = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Bot guide")
            .WithDescription(tutorialString)
            .WithColor(await DatabaseContext.UserData.Where(i => i.DiscordId == ctx.User.Id)
                           .Select(i => new DiscordColor?(i.Color))
                           .FirstOrDefaultAsync()
                       ?? TypesFunction.GetDefaultObject<UserData>().Color);

        await ctx.RespondAsync(embed);
    }
}