using System.ComponentModel;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.command;

public class TutorialCommand : GeneralCommandClass
{
    [Command("tutorial"), Description("Guide on how to use stuff"),
     AdditionalCommand("/read", BotCommandType.Fun)]
    public async Task Execute(CommandContext ctx)
    {
        const string tutorialString =
            "To start using bot, use `/begin` Commands. To see which teams you have, use `/display teams`\n" +
            "To change equipped team, use `/team equip_team` use `/help team` to see more team commands" +
            "\nTo battle a friend, use `/challenge @user`." +
            "\nNote: even though you have an option to name your character, I will not recognize your name when" +
            "querying for characters. i will only recognize \"player\"\n" +
            "eg: `/level_up jack` will not be recognized, but `level_up player` will be recognized\n" +
            "If you want to see what things you can get, use `/list_all_entities\n`" +
            "say you want to add character `lily` to your inventory, you can use" +
            "\n`give_me lily 1` which gives you one copy of lily";
        var embed = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Bot guide")
            .WithDescription(tutorialString)
            .WithColor(await DatabaseContext.UserData.FindOrCreateSelectUserDataAsync(ctx.User.Id, i => i.Color));
        await ctx.RespondAsync(embed);
    }
}