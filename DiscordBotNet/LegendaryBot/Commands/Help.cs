using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Help : GeneralCommandClass
{
    private static Dictionary<BotCommandCategory, List<Command>> _commandsThatDoesShit;

    static Help()
    {
        RefreshCommandsCategoryCache();
    }
    private static void RefreshCommandsCategoryCache()
    {
        if ((Bot.Client.GetCommandsExtension()?.Commands.Count).GetValueOrDefault(0) <= 0)
            throw new Exception("no command has been registered yet");
        _commandsThatDoesShit = [];
        foreach (var i in Bot.Client.GetCommandsExtension()!.Commands.Values)
        {
            if (i.Method is null)
            {
                foreach (var j in i.Subcommands)
                {
                    var category = ((j.Attributes.FirstOrDefault(k => k is BotCommandCategoryAttribute) as BotCommandCategoryAttribute)?
                        .BotCommandCategory).GetValueOrDefault(BotCommandCategory.Other);
                    if (!_commandsThatDoesShit.TryGetValue(category, out var commands))
                    {
                        commands = _commandsThatDoesShit[category] =  [];
                    }
                    commands.Add(j);
                }
            }
            else
            {
                var category = ((i.Attributes.FirstOrDefault(k => k is BotCommandCategoryAttribute) as BotCommandCategoryAttribute)?
                    .BotCommandCategory).GetValueOrDefault(BotCommandCategory.Other);
                if (!_commandsThatDoesShit.TryGetValue(category, out var commands))
                {
                    commands = _commandsThatDoesShit[category] =  [];
                }
                commands.Add(i);
            }
                
        }
    }
    


    public static DiscordEmbedBuilder? GenerateEmbedForCommandFailure(string cmd)
    {
        var splitted = cmd.Split(' ');
        if (!Bot.Client.GetCommandsExtension()!.Commands.TryGetValue(splitted[0], out var gottenCommand))
        {
            return null;
            
        }

        if (splitted.Length > 1)
        {
            gottenCommand = gottenCommand.Subcommands.FirstOrDefault(i => i.Name == splitted[1]);
            if (gottenCommand is null)
                return null;
        }
        var embedToBuild = new DiscordEmbedBuilder();
        var label = gottenCommand.Name;
        if (gottenCommand.Parent is not null)
        {
            label = $"{gottenCommand.Parent.Name} {label}";
        }
        foreach (var i in gottenCommand.Parameters)
        {
            var questionMark = i.DefaultValue.HasValue ? "?" : "";
            label += $" <{i.Name}{questionMark}>";
        }
        embedToBuild
            .WithTitle(label);
        embedToBuild.WithFooter("note: text with < > means it is a parameter, and ? means the parameter is optional");
        embedToBuild.WithDescription(gottenCommand.Description ?? "No Description");
        


            var additionalCommandAttribute =
               (BotCommandCategoryAttribute)  gottenCommand.Attributes.FirstOrDefault(i => i is BotCommandCategoryAttribute)!;
        
 
        foreach (var i in gottenCommand.Subcommands)
        {
            var stringToUse = i.Description ?? "No description";

            var subAttribute =       (BotCommandCategoryAttribute) i.Attributes.FirstOrDefault(j => j is BotCommandCategoryAttribute)!;
            


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

    [Command("help"),
     BotCommandCategory(BotCommandCategory.Other)]
    public async ValueTask Execute(CommandContext ctx)
    {
    var author = ctx.User;
    
    var color = await DatabaseContext
        .UserData
        .Where(i => i.Id == ctx.User.Id)
        .Select(i => new DiscordColor?(i.Color))
        .FirstOrDefaultAsync();
    if (color is null)
    {
        color = TypesFunctionality.GetDefaultObject<UserData>().Color;
    }

   
    var embedToBuild = new DiscordEmbedBuilder()
        .WithTitle("Help")
        .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
        .WithColor(color.Value)
        .WithFooter("note: text with < > means its a parameter, and ? means the command parameter is optional");




        
        Dictionary<BotCommandCategory, DiscordEmbed> embedsToUse = [];

        foreach (var i in _commandsThatDoesShit.Keys)
        {
            embedToBuild.WithTitle(i.ToString().Englishify());
            embedToBuild.ClearFields();
            foreach (var j in _commandsThatDoesShit[i])
            {
                var nameToUse = (j.Parent?.Name ??"")  + $" {j.Name}";
                
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

        IEnumerable<DiscordSelectComponentOption> enumerable()
        {
        
            foreach (var i in embedsToUse.Keys)
            {
                yield return new DiscordSelectComponentOption(i.ToString().Englishify(),
                    ((int)i).ToString());
              
            }
        }
        List<DiscordSelectComponent> components = [];
        const string selectComponentId = "select_comp";
        var selectComponent =          new DiscordSelectComponent(selectComponentId, 
            embedsToUse[BotCommandCategory.Battle].Title!,
            enumerable());


        var discordMessageBuilder = new DiscordMessageBuilder()
            .AddEmbed(embedsToUse[BotCommandCategory.Battle])
            .AddComponents(selectComponent);

        await ctx.RespondAsync(discordMessageBuilder);
        var message = (await ctx.GetResponseAsync())!;

        while (true)
        {
            var result = await message.WaitForSelectAsync(ctx.User,selectComponentId);
            if (result.TimedOut)
            {
                await message.ModifyAsync(i => i.ClearComponents());
                break;
            }
            var parsedInt =int.Parse(result.Result.Interaction.Data.Values[0]);
            var embedToUse = embedsToUse[(BotCommandCategory)parsedInt];
            selectComponent = new DiscordSelectComponent(selectComponentId, embedToUse.Title,
                selectComponent.Options);
            await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embedToUse)
                    .AddComponents(selectComponent));
        }
        
    }



       
}