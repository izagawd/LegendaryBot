using System.ComponentModel;
using System.Reflection;
using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Help : GeneralCommandClass
{

    private static void HandleDefaultHelp()
    {
                
        Dictionary<BotCommandType, StringBuilder> botCommandTypeBuilders = new();
        foreach (var i in Enum.GetValues<BotCommandType>())
            botCommandTypeBuilders[i] = new StringBuilder();
        foreach (var i in  TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<GeneralCommandClass>())
        {
            var com = i.GetType().GetCustomAttribute<CommandAttribute>();
            if (com is not null)
            {
                var additional = i.GetType().GetCustomAttribute<AdditionalCommandAttribute>();
                var type = BotCommandType.Other;
                if (additional is not null)
                    type = additional.BotCommandType;
                botCommandTypeBuilders[type].Append($"`{com.Name}`  ");
            }
            else
            {
                var selected = i.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public  | BindingFlags.Instance)
                    .Select(j =>new{Additional= j.GetCustomAttribute<AdditionalCommandAttribute>(),
                        Default = j.GetCustomAttribute<CommandAttribute>()});
                foreach (var j in selected)
                {
                    if(j.Default is null) continue;
                    var type = BotCommandType.Other;
                    if (j.Additional is not null)
                        type = j.Additional.BotCommandType;
                    botCommandTypeBuilders[type].Append($"`{j.Default.Name}`  ");
                }
            }
        }
        foreach(var i in Enum.GetValues<BotCommandType>())
            _botCommandTypeDic[i] = botCommandTypeBuilders[i].ToString();
    }


    static Help()
    {
        HandleDefaultHelp();
        HandleCommandsInformations();
    }
    private class CommandStuffHolder
    {
        public string? Example;
        public string Label;
        public string Description;
        
    }
    private class Holder

    {
        public string Name;
        public string Label;
        public string Description;
        public List<CommandStuffHolder> CommandStuffHolders =  new();
        public string? Example;


    }

    private static List<Holder> _holderList = new();


    private static IEnumerable<string> GetHelpParameterNames(MethodInfo methodInfo)
    {
        var parameters =methodInfo.GetParameters()
                    
            .Select(i => new
            {
                parameter = i,
                attribute = i.GetCustomAttribute<ParameterAttribute>()
            }).ToArray();               
        bool isFirstParam = true;
        foreach (var i in parameters)
        {
            if (isFirstParam)
            {
                isFirstParam = false;
                continue;
            }

            var paramName = i.attribute?.Name ?? i.parameter.Name ?? "";
            if (i.parameter.IsOptional)
                paramName += "?";
            yield return paramName;
        }
        
    }
    private static void Recurse(GeneralCommandClass genComClass,
        CommandAttribute attribute, List<CommandStuffHolder> stuffHolders, Type currentType = null, string? concatenator = null)
    {
        if (currentType == null) currentType = genComClass.GetType();
        if (concatenator is null) concatenator = $"{attribute.Name} ";
        foreach (var nestedType in currentType.GetNestedTypes(BindingFlags.NonPublic | 
                                                              BindingFlags.Public | BindingFlags.Instance))
        {

            var locAttribute = nestedType.GetCustomAttribute<CommandAttribute>();
            if(locAttribute is not null)
                Recurse(genComClass, locAttribute,
                    stuffHolders, nestedType ,concatenator + $"{locAttribute.Name} ");
            
        }
        var methods = currentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

    
                
        
        foreach (var j in methods)
        {
     
            var slashCom = j.GetCustomAttribute<CommandAttribute>();
            if (slashCom is not null)
            {
     
                var additional = j.GetCustomAttribute<AdditionalCommandAttribute>();
                var descr = j.GetCustomAttribute<DescriptionAttribute>();
                var stuffHolder = new CommandStuffHolder();
                if (additional is not null)
                {
                    stuffHolder.Example = additional.Example;
                }
                stuffHolder.Label = concatenator + slashCom.Name;
                bool isFirstParam = true;
                foreach (var i in GetHelpParameterNames(j))
                {
                    
                    stuffHolder.Label += $" <{i}>";
                }
                stuffHolder.Description = descr is not null ? descr.Description : "";
                stuffHolders.Add(stuffHolder);
            }

        }

    }
    private static void HandleCommandsInformations()
    {
        foreach (var i in TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<GeneralCommandClass>())
        {
            var group = i.GetType().GetCustomAttribute<CommandAttribute>();
            
        
            if (group is not null)
            {
                var holder = new Holder();
       
                
                holder.Label = group.Name;
                holder.Name = holder.Label;
                var description = i.GetType().GetCustomAttribute<DescriptionAttribute>();
                holder.Description = description?.Description ?? "";
                Recurse(i,group,holder.CommandStuffHolders);
                _holderList.Add(holder);
            }
            else
            {
                var methods = i.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                foreach (var j in methods)
                {
                    var command = j.GetCustomAttribute<CommandAttribute>();
                    if(command is null) continue;
                    var holder = new Holder();
                    holder.Label = command.Name;
                    holder.Name = command.Name;
                    foreach (var k in GetHelpParameterNames(j))
                    {
                        holder.Label += $" <{k}>";
                    }
                    var description = j.GetCustomAttribute<DescriptionAttribute>();
                    holder.Description = description is not null ? description.Description : "";
                    var additional = j.GetCustomAttribute<AdditionalCommandAttribute>();
                    if (additional is not null)
                        holder.Example = additional.Example;
                    _holderList.Add(holder);
                }
            }
            
        }
    }

    public static DiscordEmbedBuilder? GenerateEmbedForCommand(string cmd)
    {
        var holder = _holderList.FirstOrDefault(i => i.Name.ToLower() == cmd.ToLower());
        var embedToBuild = new DiscordEmbedBuilder();
        if (holder is null)
        {
            return null;
        }

        embedToBuild
            .WithTitle(holder.Label)
            .WithDescription(holder.Description);
        if (holder.Example is not null)
            embedToBuild.AddField("Example: ", holder.Example);
        foreach (var i in holder.CommandStuffHolders)
        {
            var stringToUse = $"{i.Description}";
            if (i.Example is not null && i.Example.Length > 0)
            {
                stringToUse += $"\nExamples:\n{i.Example}";
            }

            if (stringToUse.Length > 0)
                embedToBuild.AddField(i.Label, stringToUse);
        }

        return embedToBuild;

    }
    private static Dictionary<BotCommandType, string> _botCommandTypeDic = new();
    [Command("help"),
     AdditionalCommand("/help\n/help command_name",BotCommandType.Other)]
    public async ValueTask Execute(CommandContext ctx,
    [Parameter("Commands")] string? cmd = null)
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
            .WithTimestamp(DateTime.Now);
        if (cmd is null)
        {
            foreach (var i in Enum.GetValues<BotCommandType>())
            {
                embedToBuild.AddField(i.ToString().Englishify(),
                    _botCommandTypeDic[i]);
            }
        }
        else
        {
            var prevEmbed = embedToBuild;
            embedToBuild = GenerateEmbedForCommand(cmd)?
                .WithColor(color.Value)
                .WithUser(ctx.User);
            if (embedToBuild is null)
            {
                embedToBuild = prevEmbed;
                embedToBuild.WithDescription($"Command `{cmd.ToLower()}` not found");
            }
                
            
        }
        
        await ctx.RespondAsync(embed: embedToBuild.Build());

            
    }



       
}