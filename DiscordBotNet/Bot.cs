using DSharpPlus.Entities;
using System.Diagnostics;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Commands;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.VoiceNext;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Drawing.Processing;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet;




public static class Bot
{

    private static ulong SlenderId => 334412512919420928;


    private static Task FirstTimeSetupAsync()
    {



        return new PostgreSqlContext().ResetDatabaseAsync();
        
    }

    private static async Task OnMessageCreated(DiscordClient client, MessageCreatedEventArgs args)
    {
        
    }
    private static async Task StartDiscordBotAsync()
    {
        
        Client = DiscordClientBuilder.CreateDefault(ConfigurationManager.AppSettings["TestBotToken"]!,
            DiscordIntents.All)
            .ConfigureEventHandlers(i => 
                i.HandleSocketOpened(OnReady)
                    .HandleMessageCreated(OnMessageCreated))
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
        try
        {
            if (args.CommandObject is GeneralCommandClass generalCommandClass)
            {
                return generalCommandClass.AfterExecutionAsync(args.Context);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


        return Task.CompletedTask;
    }

    private static event Action idk;
    private async static Task DoShitAsync()
    {


    }
    private static async Task Main(string[] args)
    {
        await DoShitAsync();
      
        await using (var ctx = new PostgreSqlContext())
        {
            if (args.Length > 0 && args[0] == "reset-database")
            {
                Console.WriteLine("Resetting database...");
                await ctx.ResetDatabaseAsync();
                Console.WriteLine("Database reset!");
            }
            var stopwatch = new Stopwatch(); 
            Console.WriteLine("Making all users unoccupied...");
            stopwatch.Start();
            await ctx.UserData
                .ForEachAsync(i => i.IsOccupied = false);
            var count = await ctx.UserData.CountAsync();
            await ctx.SaveChangesAsync();
            Console.WriteLine($"made all users unoccupied!");
        }
        await StartDiscordBotAsync();
      
        await Website.StartAsync(args);


    }


    public static InteractivityExtension Interactivity { get; private set; } = null!;

    /// <summary>
    /// This is my discord user Id because it's too long to memorize
    /// </summary>
    public static ulong Izasid => 216230858783326209;
    /// <summary>
    /// this is the discord user Id of another account of mine that i use to test stuff
    /// </summary>
    public static ulong Testersid => 266157684380663809;

    public static ulong Surjidid => 1025325026955767849;
    public static DiscordClient Client { get; private set; }



    public static string GlobalFontName => "Arial";

    
    private static async  Task OnCommandError(CommandsExtension extension,CommandErroredEventArgs args)
    {
        Console.WriteLine(args.Exception);


        try
        {
            DiscordColor color;
            var commandClass = args.CommandObject as GeneralCommandClass;
            if (commandClass is not null)
            {
                color = await commandClass.DatabaseContext.UserData
                    .Where(i => i.Id == args.Context.User.Id)
                    .Select(i => i.Color)
                    .FirstOrDefaultAsync();
           
                await commandClass.AfterExecutionAsync(args.Context);
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
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
       
    }
    

    private static Task OnReady(DiscordClient client, SocketEventArgs e)
    {
        Console.WriteLine("Ready!");
        return Task.CompletedTask;
    }
}