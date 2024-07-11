using System.Reflection;
using System.Text;
using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Primitives;

namespace DiscordBotNet.LegendaryBot.command;

public class Help : GeneralCommandClass
{

    private static void HandleDefaultHelp()
    {
                
        Dictionary<BotCommandType, StringBuilder> botCommandTypeBuilders = new();
        foreach (var i in Enum.GetValues<BotCommandType>())
            botCommandTypeBuilders[i] = new StringBuilder();
        foreach (var i in  DefaultObjects.GetDefaultObjectsThatSubclass<GeneralCommandClass>())
        {
            var slashCom = i.GetType().GetCustomAttribute<SlashCommandGroupAttribute>();
            if (slashCom is not null)
            {
                var additional = i.GetType().GetCustomAttribute<AdditionalSlashCommandAttribute>();
                BotCommandType type = BotCommandType.Other;
                if (additional is not null)
                    type = additional.BotCommandType;
                botCommandTypeBuilders[type].Append($"{slashCom.Name}  ");
            }
            else
            {
                var selected = i.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public  | BindingFlags.Instance)
                    .Select(j =>new{Additional= j.GetCustomAttribute<AdditionalSlashCommandAttribute>(),
                        Default = j.GetCustomAttribute<SlashCommandAttribute>()});
                foreach (var j in selected)
                {
                    if(j.Default is null) continue;
                    BotCommandType type = BotCommandType.Other;
                    if (j.Additional is not null)
                        type = j.Additional.BotCommandType;
                    botCommandTypeBuilders[type].Append($"{j.Default.Name}  ");
                }
            }
        }
        foreach(var i in Enum.GetValues<BotCommandType>())
            _botCommandTypeDic[i] = botCommandTypeBuilders[i].ToString();
    }
    /// <summary>
    /// Must be called at start of program
    /// </summary>
    public static void LoadHelpMenu()
    {
        HandleDefaultHelp();
        HandleCommandsInformations();
    }

    private class CommandStuffHolder
    {
        public string? Example;
        public string Name;
        public string Description;
        
    }
    private class Holder

    {
        public string Name;
        public string Description;
        public List<CommandStuffHolder> CommandStuffHolders =  new();
        public string? Example;


    }

    private static List<Holder> _holderList = new();

    private static void Recurse(GeneralCommandClass genComClass,
        SlashCommandGroupAttribute attribute, List<CommandStuffHolder> stuffHolders, Type currentType = null, string? concatenator = null)
    {
        if (currentType == null) currentType = genComClass.GetType();
        if (concatenator is null) concatenator = $"{attribute.Name} ";
        foreach (var nestedType in currentType.GetNestedTypes(BindingFlags.NonPublic | 
                                                              BindingFlags.Public | BindingFlags.Instance))
        {

            var locAttribute = nestedType.GetCustomAttribute<SlashCommandGroupAttribute>();
            if(locAttribute is not null)
                Recurse(genComClass, locAttribute,
                    stuffHolders, nestedType ,concatenator + $"{locAttribute.Name} ");
            
        }
        var methods = currentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

    
                
        foreach (var j in methods)
        {
            var slashCom = j.GetCustomAttribute<SlashCommandAttribute>();
            if(slashCom is null) continue;
            var additional = j.GetCustomAttribute<AdditionalSlashCommandAttribute>();
            var stuffHolder = new CommandStuffHolder();
            if (additional is not null)
            {
                stuffHolder.Example = additional.Example;
            }
            stuffHolder.Name = concatenator + slashCom.Name;
            stuffHolder.Description = slashCom.Description;
            stuffHolders.Add(stuffHolder);
        }

    }
    private static void HandleCommandsInformations()
    {
        foreach (var i in DefaultObjects.GetDefaultObjectsThatSubclass<GeneralCommandClass>())
        {
            var group = i.GetType().GetCustomAttribute<SlashCommandGroupAttribute>();
            
          
            if (group is not null)
            {
                var holder = new Holder();
                holder.Name = group.Name;
                holder.Description = group.Description;
                Recurse(i,group,holder.CommandStuffHolders);
                _holderList.Add(holder);
            }
            else
            {
                var methods = i.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                foreach (var j in methods)
                {
                    var command = j.GetCustomAttribute<SlashCommandAttribute>();
                    if(command is null) continue;
                    var holder = new Holder();
                    holder.Name = command.Name;
                    holder.Description = command.Description;
                    var additional = j.GetCustomAttribute<AdditionalSlashCommandAttribute>();
                    if (additional is not null)
                        holder.Example = additional.Example;
                    _holderList.Add(holder);
                }
            }
            
        }
    }
    private static Dictionary<BotCommandType, string> _botCommandTypeDic = new();
    [SlashCommand("help","the help"),
     AdditionalSlashCommand("/help\n/help command_name",BotCommandType.Other)]
    public async Task Execute(InteractionContext ctx,
    [Option("command","put if you want to check information about a command")] string? cmd = null)
    {
        
        DiscordUser author = ctx.User;

        DiscordColor color = await DatabaseContext
            .UserData
            .FindOrCreateSelectUserDataAsync((long)author.Id, 
                i => i.Color);

        DiscordEmbedBuilder embedToBuild = new DiscordEmbedBuilder()
            .WithTitle("Help")
            .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
            .WithColor(color)
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
            var holder = _holderList.FirstOrDefault(i => i.Name.ToLower() == cmd.ToLower());
            if(holder is null) return;
            embedToBuild
                .WithTitle(holder.Name)
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
                embedToBuild.AddField(i.Name,stringToUse );
            }
        }
        
        await ctx.CreateResponseAsync(embed: embedToBuild.Build());

            
    }



       
}