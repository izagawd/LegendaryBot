using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands;

public class Help : GeneralCommandClass
{
    private static readonly Dictionary<BotCommandCategory, List<Command>> CommandsThatDoesShit = [];

    static Help()
    {
        RefreshCommandsCategoryCache();
    }

    private static void RefreshCommandsCategoryCache()
    {
        var commandsExtension = DiscordBot.Client.ServiceProvider.GetRequiredService<CommandsExtension>();
        if (commandsExtension.Commands.Count <= 0)
            throw new Exception("no command has been registered yet");

        foreach (var i in commandsExtension.Commands.Values)
            if (i.Method is null)
            {
                foreach (var j in i.Subcommands)
                {
                    var category =
                        ((j.Attributes.FirstOrDefault(k => k is BotCommandCategoryAttribute) as
                                BotCommandCategoryAttribute)?
                            .BotCommandCategory).GetValueOrDefault(BotCommandCategory.Other);
                    if (!CommandsThatDoesShit.TryGetValue(category, out var commands))
                        commands = CommandsThatDoesShit[category] = [];
                    commands.Add(j);
                }
            }
            else
            {
                var category =
                    ((i.Attributes.FirstOrDefault(k => k is BotCommandCategoryAttribute) as BotCommandCategoryAttribute)
                        ?
                        .BotCommandCategory).GetValueOrDefault(BotCommandCategory.Other);
                if (!CommandsThatDoesShit.TryGetValue(category, out var commands))
                    commands = CommandsThatDoesShit[category] = [];
                commands.Add(i);
            }
    }


    public static DiscordEmbedBuilder? GenerateEmbedForCommandFailure(string cmd)
    {
        var splitted = cmd.Split(' ');
        if (!DiscordBot.Client.ServiceProvider.GetRequiredService<CommandsExtension>().Commands.TryGetValue(
                splitted[0], out var gottenCommand)) return null;

        if (splitted.Length > 1)
        {
            gottenCommand = gottenCommand.Subcommands.FirstOrDefault(i => i.Name == splitted[1]);
            if (gottenCommand is null)
                return null;
        }

        var embedToBuild = new DiscordEmbedBuilder();
        var label = gottenCommand.Name;
        if (gottenCommand.Parent is not null) label = $"{gottenCommand.Parent.Name} {label}";
        foreach (var i in gottenCommand.Parameters)
        {
            var questionMark = i.DefaultValue.HasValue ? "?" : "";
            label += $" <{i.Name}{questionMark}>";
        }

        embedToBuild
            .WithTitle(label);
        embedToBuild.WithFooter("note: text with < > means it is a parameter, and ? means the parameter is optional");
        embedToBuild.WithDescription(gottenCommand.Description ?? "No Description");


        foreach (var i in gottenCommand.Subcommands)
        {
            var stringToUse = i.Description ?? "No description";


            var subLabel = $"{gottenCommand.Name} {i.Name}";
            foreach (var j in i.Parameters)
            {
                var questionMark = j.DefaultValue.HasValue ? "?" : "";
                subLabel += $" <{j.Name}{questionMark}>";
            }

            embedToBuild.AddField(subLabel, stringToUse);
        }

        return embedToBuild;
    }

    [Command("help")]
    [BotCommandCategory]
    public async ValueTask Execute(CommandContext ctx)
    {
        var author = ctx.User;

        var color = await DatabaseContext
            .Set<UserData>()
            .Where(i => i.DiscordId == ctx.User.Id)
            .Select(i => new DiscordColor?(i.Color))
            .FirstOrDefaultAsync();
        if (color is null) color = TypesFunction.GetDefaultObject<UserData>().Color;


        var embedToBuild = new DiscordEmbedBuilder()
            .WithTitle("Help")
            .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
            .WithColor(color.Value)
            .WithFooter("note: text with < > means its a parameter, and ? means the command parameter is optional");


        Dictionary<BotCommandCategory, DiscordEmbed> embedsToUse = [];

        foreach (var i in CommandsThatDoesShit.Keys)
        {
            embedToBuild.WithTitle(i.ToString());
            embedToBuild.ClearFields();
            foreach (var j in CommandsThatDoesShit[i])
            {
                var nameToUse = (j.Parent?.Name ?? "") + $" {j.Name}";

                foreach (var k in j.Parameters)
                {
                    var questionMark = k.DefaultValue.HasValue ? "?" : "";
                    nameToUse += $" <{k.Name}{questionMark}>";
                }

                var otherInfo = j.Description ?? "No Description";

                embedToBuild.AddField(nameToUse, otherInfo);
            }

            embedsToUse[i] = embedToBuild.Build();
        }

        IEnumerable<DiscordSelectComponentOption> TheEnumerable()
        {
            foreach (var i in embedsToUse.Keys.OrderBy(i =>
                     {
                         switch (i)
                         {
                             case BotCommandCategory.Battle:
                                 return (short)0;
                             case BotCommandCategory.Other:
                                 return (short)2;
                             default:
                                 return (short)1;
                         }
                     }))
                yield return new DiscordSelectComponentOption(i.GetName(),
                    ((int)i).ToString());
        }

        const string selectComponentId = "select_comp";
        var selectComponent = new DiscordSelectComponent(selectComponentId,
            embedsToUse[BotCommandCategory.Battle].Title!,
            TheEnumerable());


        var discordMessageBuilder = new DiscordMessageBuilder()
            .AddEmbed(embedsToUse[BotCommandCategory.Battle])
            .AddComponents(selectComponent);

        await ctx.RespondAsync(discordMessageBuilder);
        var message = (await ctx.GetResponseAsync())!;

        while (true)
        {
            var result = await message.WaitForSelectAsync(ctx.User, selectComponentId);
            if (result.TimedOut)
            {
                await message.ModifyAsync(i => i.Components.SelectMany(j => j.Components)
                    .OfType<DiscordSelectComponent>().ForEach(j => j.Disable()));
                break;
            }

            var parsedInt = int.Parse(result.Result.Interaction.Data.Values[0]);
            var embedToUse = embedsToUse[(BotCommandCategory)parsedInt];
            selectComponent = new DiscordSelectComponent(selectComponentId, embedToUse.Title ?? "",
                selectComponent.Options);
            await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embedToUse)
                    .AddComponents(selectComponent));
        }
    }
}