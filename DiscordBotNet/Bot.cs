using System.Collections.Concurrent;
using System.Collections.Immutable;
using DSharpPlus.Entities;
using System.Diagnostics;
using System.Linq.Expressions;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Commands;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands.Trees;
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

    struct ChannelSpawnInfo
    {
        public int MessageCount;
        public DateTime LastTimeIncremented = DateTime.UtcNow.AddDays(-1);
        
        public ChannelSpawnInfo(){}
    }


    const int messagesTillExecution = 30;
    private const float messageCoolDown = 1.5f;
    private static ConcurrentDictionary<ulong, ChannelSpawnInfo> idkDictionary = new();

    
    
    private static ConcurrentDictionary<ulong, CharacterExpGainInfo> expMatGive = new();
    struct CharacterExpGainInfo
    {
        public int MessageCount;
        public DateTime LastTimeIncremented = DateTime.UtcNow.AddDays(-1);
        
        public CharacterExpGainInfo(){}
    }
    


    private static async Task OnMessageCreatedGiveUserExpMat(DiscordClient client, MessageCreatedEventArgs args)
    {

        
        var permissions = args.Guild.CurrentMember.PermissionsIn(args.Channel);
        if (!permissions.HasFlag(DiscordPermissions.EmbedLinks) || !permissions.HasFlag(DiscordPermissions.SendMessages))
        {
            return;
        }
        var expGainInfo = expMatGive.GetOrAdd(args.Author.Id, new CharacterExpGainInfo());
      
        if (DateTime.UtcNow.Subtract(expGainInfo.LastTimeIncremented).Seconds >= messageCoolDown)
        {
           
            expGainInfo.MessageCount++;
            expGainInfo.LastTimeIncremented = DateTime.UtcNow;
            expMatGive[args.Author.Id] = expGainInfo;
            if (expGainInfo.MessageCount >= messagesTillExecution)
            {
                expGainInfo.MessageCount = 0;
                expMatGive[args.Author.Id] = expGainInfo;
                await using var dbContext = new PostgreSqlContext();
                var userData =await  dbContext.UserData
                    .Include(i => i.Items.Where(j => j is CharacterExpMaterial))
                    .FirstOrDefaultAsync(i => i.Id == args.Author.Id);
                if(userData is null || userData.Tier <= Tier.Unranked)
                    return;
                List<CharacterExpMaterial> characterExpMaterials = [];
                foreach (var i in Enumerable.Range(0,(int) userData.Tier * 3) )
                {
                    characterExpMaterials.Add(new AdventurersKnowledge());
                }

                var rewardString = userData.ReceiveRewards(new EntityReward(characterExpMaterials));
                await dbContext.SaveChangesAsync();
                var embed = new DiscordEmbedBuilder()
                    .WithColor(userData.Color)
                    .WithUser(args.Author)
                    .WithTitle($"{args.Author.Username} gained some exp rewards for being active!")
                    .WithDescription(rewardString);
                await args.Channel.SendMessageAsync(embed);

            }
        }
    
        

    }
    private static async Task OnMessageCreatedSpawnCharacter(DiscordClient client, MessageCreatedEventArgs args)
    {        
        var permissions = args.Guild.CurrentMember.PermissionsIn(args.Channel);
        if (!permissions.HasFlag(DiscordPermissions.EmbedLinks) || !permissions.HasFlag(DiscordPermissions.SendMessages))
        {
            return;
        }
        if (!args.Author.IsBot)
        {
            var spawnInfo = idkDictionary.GetOrAdd(args.Channel.Id,
                new ChannelSpawnInfo());
          
            if (DateTime.UtcNow.Subtract(spawnInfo.LastTimeIncremented).Seconds >= messageCoolDown)
            {
                spawnInfo.MessageCount++;
                spawnInfo.LastTimeIncremented = DateTime.UtcNow;
                idkDictionary[args.Channel.Id] = spawnInfo;
                if (spawnInfo.MessageCount >= messagesTillExecution)
                {
                    spawnInfo.MessageCount = 0;
                    idkDictionary[args.Channel.Id] = spawnInfo;
                    var groups = TypesFunctionality
                        .GetDefaultObjectsThatIsInstanceOf<Character>()
                        .Where(i =>  i.CanSpawnNormally)
                        .GroupBy(i => i.Rarity)
                        .ToImmutableArray();
         
                    var groupToUse = BasicFunctionality.GetRandom(new Dictionary<IGrouping<Rarity, Character>, double>()
                    {
                        {groups.First(i => i.Key == Rarity.ThreeStar),85},
                        {groups.First(i => i.Key == Rarity.FourStar), 14},
                        {groups.First(i => i.Key == Rarity.FiveStar),1}
                    });
                
                    var randomCharacterType 
                        = BasicFunctionality.RandomChoice(groupToUse.Select(i => i)).GetType();
                    var created = (Character)Activator.CreateInstance(randomCharacterType)!;
                    await using var stream = new MemoryStream();
                    using var image = await BasicFunctionality.GetImageFromUrlAsync(created.ImageUrl);
                    image.Mutate(i => i.Resize(200,200));
                    await image.SaveAsPngAsync(stream);
                    stream.Position = 0;
                    var claimCharacter = new DiscordButtonComponent(
        DiscordButtonStyle.Success,
        "claim", "CLAIM");
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(created.Color)
                        .WithTitle("Character has appeared!\nThey will join one with the fastest reaction time")
                        .WithDescription($"Name: {created.Name}\nRarity: {(int) created.Rarity} :star:")
                        .WithImageUrl("attachment://character.png");
                    var message = await args.Channel.SendMessageAsync(new DiscordMessageBuilder()
                        .AddEmbed(embed)
                        .AddComponents(claimCharacter)
                        .AddFile("character.png",stream));
                    var result =await message.WaitForButtonAsync();
                    claimCharacter.Disable();
                    
                    if (result.TimedOut)
                    {
                        
                        await message.ModifyAsync(new DiscordMessageBuilder()
                            .AddEmbed(embed)
                            .AddComponents(claimCharacter));
                        return;
                    }
                        
                    var localUser = result.Result.User;
                    await using var postgre = new PostgreSqlContext();
                    var userData = await postgre.UserData.FirstOrDefaultAsync(i => i.Id == localUser.Id);
                    bool isNew = userData is null || userData.Tier == Tier.Unranked;
                    if (userData is null)
                    {
                        userData = new UserData(localUser.Id);
                        await postgre.UserData.AddAsync(userData);
                    }
                    userData.Characters.Add(created);
                    await postgre.SaveChangesAsync();
                    var text = $"{localUser.Mention} claimed {created.Name}!";
                    if (isNew)
                        text += "Seems like you dont battle. Consider joining!";
                                 
                    await message.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed)
                        .AddComponents(claimCharacter));
                    
                     embed = new DiscordEmbedBuilder()
                        .WithUser(localUser)
                        .WithTitle("Success!")
                        .WithColor(userData.Color)
                        .WithDescription(text);
      
                    await result.Result.Interaction.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .AddEmbed(embed));
                    

                }
            }
        }
    }


    private static async Task StartDiscordBotAsync()
    {
        
        Client = DiscordClientBuilder.CreateDefault(ConfigurationManager.AppSettings["BotToken"]!,
            DiscordIntents.All)
            .ConfigureEventHandlers(i => 
                i.HandleSocketOpened(OnReady)
                    .HandleMessageCreated(OnMessageCreatedSpawnCharacter)
                    .HandleMessageCreated(OnMessageCreatedGiveUserExpMat))
            
            .Build();

        var commandsExtension = Client.UseCommands(new CommandsConfiguration()
        {
            UseDefaultCommandErrorHandler = false
        });
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
        
        commandsExtension.AddCommands(typeof(Bot).Assembly);
        commandsExtension.CommandExecuted += OnCommandsExtensionOnCommandExecuted;


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


    class Bruh : IBattleEventListener
    {
        [BattleEventListenerMethod]
        public void A(TurnEndEventArgs a)
        {
            
        }
    }

    private async static Task DoShitAsync()
    {
        await new PostgreSqlContext().ResetDatabaseAsync();
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

 
   
        
        var text = $"Send this error to my creator\n```{args.Exception}```";
        if (args.CommandObject is GeneralCommandClass commandClass)
        {
            await commandClass.AfterExecutionAsync(args.Context);
        }

        DiscordEmbedBuilder? embedBuilder = null;
        await using var dbContext = new PostgreSqlContext();
        var color  = (await dbContext.UserData
                .Where(i => i.Id == args.Context.User.Id)
                .Select(i => new DiscordColor?(i.Color))
                .FirstOrDefaultAsync())
            .GetValueOrDefault(TypesFunctionality.GetDefaultObject<UserData>().Color);
        if (args.Exception is CommandNotFoundException exception)
        {
            text = $"Command `{exception.CommandName}` not found";
        } else if (args.Exception is CommandsException)
        {

            var commandToUse = args.Context.Command;
            while (commandToUse.Parent is not null)
            {
                commandToUse = commandToUse.Parent;
            }
            embedBuilder = Help.GenerateEmbedForCommand(commandToUse.Name);
  
            embedBuilder?.WithTitle($"You didnt properly use command `{commandToUse.Name}`.\nThis is how to use `{commandToUse.Name}`\n"
                + embedBuilder.Title);
          
        }



        if (embedBuilder is null)
            embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Hmm")
                .WithDescription(text);
        embedBuilder.WithColor(color)
            .WithUser(args.Context.User);
            await args.Context.Channel.SendMessageAsync(embedBuilder);

       
    }
    

    private static Task OnReady(DiscordClient client, SocketEventArgs e)
    {
        Console.WriteLine("Ready!");
        return Task.CompletedTask;
    }
}