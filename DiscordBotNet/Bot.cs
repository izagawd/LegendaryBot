using System.ComponentModel;
using DSharpPlus.Entities;
using System.Diagnostics;
using System.Reflection;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.command;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands.Processors.UserCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Net.Models;
using DSharpPlus.VoiceNext;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet;




public static class Bot
{

    private static long SlenderId => 334412512919420928;


    private static  Task FirstTimeSetupAsync()
    {

        return new PostgreSqlContext().ResetDatabaseAsync();
    }



    private static async Task StartDiscordBotAsync()
    {
      
        
        Client = DiscordClientBuilder.CreateDefault(ConfigurationManager.AppSettings["BotToken"]!,
            DiscordIntents.All)
            .ConfigureEventHandlers(i => i.HandleSocketOpened(OnReady))
            .Build();

        var commandsExtension = Client.UseCommands();
        var slashCommandProcessor = new SlashCommandProcessor();
    
        slashCommandProcessor.AddConverters(typeof(Bot).Assembly);
        
        await commandsExtension.AddProcessorAsync(slashCommandProcessor);
        TextCommandProcessor textCommandProcessor = new(new()
        {
            
            // The default behavior is that the bot reacts to direct mentions
            // and to the "!" prefix.
            // If you want to change it, you first set if the bot should react to mentions
            // and then you can provide as many prefixes as you want.
            PrefixResolver = new DefaultPrefixResolver(true, "?", "&").ResolvePrefixAsync,
            IgnoreBots = true,
        });
        
        
        textCommandProcessor.AddConverters(typeof(Bot).Assembly);
        await commandsExtension.AddProcessorAsync(textCommandProcessor);
        DefaultCommandExecutor idk;

        commandsExtension.CommandExecuted += OnCommandsExtensionOnCommandExecuted;
        
        commandsExtension.AddCommands(typeof(Bot).Assembly);
   
        commandsExtension.CommandErrored += OnCommandError;
        
        Client.UseVoiceNext(new VoiceNextConfiguration { AudioFormat = AudioFormat.Default});
        var interactivityConfiguration = new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(2),
        };
        Interactivity = Client.UseInteractivity(interactivityConfiguration);
     
       
        await Client.ConnectAsync();
    }

    private static  Task OnCommandsExtensionOnCommandExecuted(CommandsExtension sender, CommandExecutedEventArgs args)
    {
        if (args.CommandObject is GeneralCommandClass generalCommandClass)
        {
            return generalCommandClass.AfterSlashExecutionAsync(args.Context);
        }

        return Task.CompletedTask;
    }

    private static Task DoShitAsync()
    {
        var idk = new Player();



        idk.Level = 60;
        idk.Ascension = 6;
        idk.LoadGear();
        idk.Defense.Print();
        idk.ExpToGainWhenKilled.Print();
        idk.GetRequiredExperienceToNextLevel().Print();

        return Task.CompletedTask;

    }
    private static async Task Main(string[] args)
    {
        await DoShitAsync();
        var stopwatch = new Stopwatch(); 
        Console.WriteLine("Making all users unoccupied...");
        stopwatch.Start();
        await using (var ctx = new PostgreSqlContext())
        {
            await ctx.UserData
                .ForEachAsync(i => i.IsOccupied = false);
            var count = await ctx.UserData.CountAsync();
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Took a total of {stopwatch.Elapsed.TotalMilliseconds}ms to make {count} users unoccupied");
        }
        await StartDiscordBotAsync();
        await Website.StartAsync(args);


    }


    public static InteractivityExtension Interactivity { get; private set; } = null!;

    /// <summary>
    /// This is my discord user Id because it's too long to memorize
    /// </summary>
    public static long Izasid => 216230858783326209;
    /// <summary>
    /// this is the discord user Id of another account of mine that i use to test stuff
    /// </summary>
    public static long Testersid => 266157684380663809;

    public static long Surjidid => 1025325026955767849;
    public static DiscordClient Client { get; private set; }



    public static string GlobalFontName => "Arial";

    
    private static async  Task OnCommandError(CommandsExtension extension,CommandErroredEventArgs args)
    {
        Console.WriteLine(args.Exception);


        DiscordColor color;
        var commandClass = args.CommandObject as GeneralCommandClass;
        if (commandClass is not null)
        {
            color = await commandClass.DatabaseContext.UserData.FindOrCreateSelectUserDataAsync(
                (long)args.Context.User.Id, i => i.Color);
            await commandClass.AfterSlashExecutionAsync(args.Context);
        }
        else
        {
            color = DefaultObjects.GetDefaultObject<UserData>().Color;
        }
        
        var embed = new DiscordEmbedBuilder()
            .WithColor(color)
            .WithTitle("hmm")
            .WithDescription("Something went wrong").Build();

        await args.Context.Channel.SendMessageAsync(embed);
    }
    

    private static Task OnReady(DiscordClient client, SocketEventArgs e)
    {
        Console.WriteLine("Ready!");
        return Task.CompletedTask;
    }
}