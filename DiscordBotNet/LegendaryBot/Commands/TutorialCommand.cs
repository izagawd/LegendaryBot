using System.ComponentModel;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class TutorialCommand : GeneralCommandClass
{
    [Command("tutorial"), Description("Guide on how to use stuff"),
     AdditionalCommand("/read", BotCommandType.Fun)]
    public async Task Execute(CommandContext ctx)
    {
        const string tutorialString =
            "To start using bot, use `/begin` Commands. To see which teams you have, use `/display teams`\n" +
            "To change equipped team, use `/team equip-team` use `/help team` to see more team commands" +
            "\nTo battle a friend, use `/challenge @user`." +
            "\nNote: even though you have an option to name your character, I will not recognize your name when" +
            "querying for characters. i will only recognize \"player\"\n" +
            "eg: `/level-up character jack` will not be recognized, but `level-up character player` will be recognized\n" +
            "If you want to see what things you can get, use `/list-all-entities\n`" +
            "say you want to add character `lily` to your inventory, you can use" +
            "\n`give-me lily 1` which gives you one copy of lily" +
            "\n to fight against any character, you can use the /hunt command" +
            "\nNote: a blessing is like a weapon from genshin, or you could say a lightcone from honkai star rail";
        var embed = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Bot guide")
            .WithDescription(tutorialString)
            .WithColor((await DatabaseContext.UserData.Where(i => i.Id == ctx.User.Id)
                .Select(i => new DiscordColor?(i.Color))
                .FirstOrDefaultAsync())
                .GetValueOrDefault(ObjectsFunctionality.GetDefaultObject<UserData>().Color));
        await ctx.RespondAsync(embed);
    }
}