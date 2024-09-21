using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Configuration;
using BasicFunctionality;
using DatabaseManagement;
using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using PublicInfo;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DiscordBot;

public class DiscordBot
{
    public const string BotTokenToPathUse = Information.IsTesting ? "TestBotToken" : "BotToken";

    /// <summary>
    ///     This is my discord user Id because it's too long to memorize
    /// </summary>
    public const ulong Izasid = 216230858783326209;

    /// <summary>
    ///     this is the discord user Id of another account of mine that i use to test stuff
    /// </summary>
    public const ulong Testersid = 266157684380663809;

    public const ulong Surjidid = 1025325026955767849;

    private const ulong SlenderId = 334412512919420928;


    private const int MessagesTillExecution = 30;
    private const float MessageCoolDown = 1.5f;
    private static readonly ConcurrentDictionary<ulong, ChannelSpawnInfo> ChannelSpawnInfoDictionary = new();

    private static readonly ConcurrentDictionary<ulong, CharacterExpGainInfo> ExpMatGive = new();


    public static DiscordClient Client { get; private set; } = null!;


    private static async Task OnCommandError(CommandsExtension extension, CommandErroredEventArgs args)
    {
        Console.WriteLine(args.Exception);


        var text = $"Send this error to my creator\n```{args.Exception}```";
        if (args.CommandObject is GeneralCommandClass commandClass)
            await commandClass.AfterExecutionAsync(args.Context);

        DiscordEmbedBuilder? embedBuilder = null;
        await using var dbContext = new PostgreSqlContext();
        var color = (await dbContext.Set<UserData>()
                .Where(i => i.DiscordId == args.Context.User.Id)
                .Select(i => new DiscordColor?(i.Color))
                .FirstOrDefaultAsync())
            .GetValueOrDefault(TypesFunction.GetDefaultObject<UserData>().Color);
        if (args.Exception is CommandNotFoundException exception)
        {
            text = $"Command `{exception.CommandName}` not found";
        }
        else if (args.Exception is CommandsException)
        {
            var currentCommand = args.Context.Command;

            var nameToUse = currentCommand.Name;
            while (currentCommand.Parent is not null)
            {
                currentCommand = currentCommand.Parent;
                nameToUse = currentCommand.Name + " " + nameToUse;
            }

            embedBuilder = Help.GenerateEmbedForCommandFailure(nameToUse);

            embedBuilder?.WithTitle($"You didnt properly use command `{nameToUse}`.\nDetails of `{nameToUse}`\n"
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

    private static async Task OnMessageCreatedGiveUserExpMat(DiscordClient client, MessageCreatedEventArgs args)
    {
        if (args.Guild is null) return;
        var permissions = args.Guild.CurrentMember.PermissionsIn(args.Channel);
        if (!permissions.HasFlag(DiscordPermissions.EmbedLinks) ||
            !permissions.HasFlag(DiscordPermissions.SendMessages)) return;
        var expGainInfo = ExpMatGive.GetOrAdd(args.Author.Id, new CharacterExpGainInfo());

        if (DateTime.UtcNow.Subtract(expGainInfo.LastTimeIncremented).Seconds >= MessageCoolDown)
        {
            expGainInfo.MessageCount++;
            expGainInfo.LastTimeIncremented = DateTime.UtcNow;
            ExpMatGive[args.Author.Id] = expGainInfo;
            if (expGainInfo.MessageCount >= MessagesTillExecution)
            {
                expGainInfo.MessageCount = 0;
                ExpMatGive[args.Author.Id] = expGainInfo;
                await using var dbContext = new PostgreSqlContext();
                var userData = await dbContext.Set<UserData>()
                    .Include(i => i.Items.Where(j => j is CharacterExpMaterial))
                    .FirstOrDefaultAsync(i => i.DiscordId == args.Author.Id);
                if (userData is null || userData.Tier <= Tier.Unranked)
                    return;
                List<CharacterExpMaterial> characterExpMaterials = [];
                foreach (var _ in Enumerable.Range(0, (int)userData.Tier * 3))
                    characterExpMaterials.Add(new AdventurersKnowledge { Stacks = 1 });

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
        if (args.Guild is null) return;
        var permissions = args.Guild.CurrentMember.PermissionsIn(args.Channel);
        if (!permissions.HasFlag(DiscordPermissions.EmbedLinks) ||
            !permissions.HasFlag(DiscordPermissions.SendMessages)) return;
        if (!args.Author.IsBot)
        {
            var spawnInfo = ChannelSpawnInfoDictionary.GetOrAdd(args.Channel.Id,
                new ChannelSpawnInfo());

            if (DateTime.UtcNow.Subtract(spawnInfo.LastTimeIncremented).Seconds >= MessageCoolDown)
            {
                spawnInfo.MessageCount++;
                spawnInfo.LastTimeIncremented = DateTime.UtcNow;
                ChannelSpawnInfoDictionary[args.Channel.Id] = spawnInfo;
                if (spawnInfo.MessageCount >= MessagesTillExecution)
                {
                    spawnInfo.MessageCount = 0;
                    ChannelSpawnInfoDictionary[args.Channel.Id] = spawnInfo;
                    var groups = TypesFunction
                        .GetDefaultObjectsAndSubclasses<Character>()
                        .Where(i => i.CanSpawnNormally)
                        .GroupBy(i => i.Rarity)
                        .ToImmutableArray();

                    var groupToUse = BasicFunctions.GetRandom(new Dictionary<IGrouping<Rarity, Character>, double>
                    {
                        { groups.First(i => i.Key == Rarity.ThreeStar), 85 },
                        { groups.First(i => i.Key == Rarity.FourStar), 14 },
                        { groups.First(i => i.Key == Rarity.FiveStar), 1 }
                    });

                    var randomCharacterType
                        = BasicFunctions.RandomChoice(groupToUse.Select(i => i)).GetType();
                    var created = (Character)Activator.CreateInstance(randomCharacterType)!;
                    await using var stream = new MemoryStream();
                    using var image = await ImageFunctions.GetImageFromUrlAsync(created.ImageUrl);
                    image.Mutate(i => i.Resize(200, 200));
                    await image.SaveAsPngAsync(stream);
                    stream.Position = 0;
                    var claimCharacter = new DiscordButtonComponent(
                        DiscordButtonStyle.Success,
                        "claim", "CLAIM");
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(created.Color)
                        .WithTitle("Character has appeared!\nThey will join one with the fastest reaction time")
                        .WithDescription(
                            $"Name: {created.Name}\n{string.Concat(Enumerable.Repeat("\u2b50", (int)created.Rarity))}")
                        .WithImageUrl("attachment://character.png");
                    var message = await args.Channel.SendMessageAsync(new DiscordMessageBuilder()
                        .AddEmbed(embed)
                        .AddComponents(claimCharacter)
                        .AddFile("character.png", stream));
                    var result = await message.WaitForButtonAsync();
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
                    var userData = await postgre.Set<UserData>()
                        .Include(i => i.Characters.Where(j => j.TypeId == created.TypeId))
                        .FirstOrDefaultAsync(i => i.DiscordId == localUser.Id);
                    var isNew = userData is null || userData.Tier == Tier.Unranked;
                    if (userData is null)
                    {
                        userData = new UserData(localUser.Id);
                        await postgre.Set<UserData>().AddAsync(userData);
                    }
                    var text =  userData.ReceiveRewards([new EntityReward([created])]);
                    await postgre.SaveChangesAsync();
                
                    if (isNew)
                        text += "\nSeems like you dont battle. Consider joining!";

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


    public static async Task StartDiscordBotAsync()
    {
        Client = DiscordClientBuilder.CreateDefault(ConfigurationManager.AppSettings[BotTokenToPathUse]!,
                DiscordIntents.All)
            .UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            })
            .UseCommands(commandsExtension =>
            {
                var slashCommandProcessor = new SlashCommandProcessor();
                slashCommandProcessor.AddConverters(typeof(DiscordBot).Assembly);
                commandsExtension.AddProcessor(slashCommandProcessor);
                TextCommandProcessor textCommandProcessor = new(new TextCommandConfiguration
                {
                    // The default behavior is that the bot reacts to direct mentions
                    // and to the "!" prefix.
                    // If you want to change it, you first set if the bot should react to mentions
                    // and then you can provide as many prefixes as you want.
                    PrefixResolver = new DefaultPrefixResolver(true, "&").ResolvePrefixAsync,
                    IgnoreBots = true
                });
                textCommandProcessor.AddConverters(typeof(DiscordBot).Assembly);
                commandsExtension.AddProcessor(textCommandProcessor);
                commandsExtension.AddCommands(typeof(DiscordBot).Assembly);
                commandsExtension.CommandExecuted += OnCommandsExtensionOnCommandExecuted;
                commandsExtension.CommandErrored += OnCommandError;
            }, new CommandsConfiguration
            {
                UseDefaultCommandErrorHandler = false
            })
            .ConfigureEventHandlers(i =>
                i.HandleSocketOpened(OnReady)
                    .HandleMessageCreated(OnMessageCreatedSpawnCharacter)
                    .HandleMessageCreated(OnMessageCreatedGiveUserExpMat))
            .Build();


        await Client.ConnectAsync();
        
    }


    private static Task OnCommandsExtensionOnCommandExecuted(CommandsExtension sender, CommandExecutedEventArgs args)
    {
        try
        {
            if (args.CommandObject is GeneralCommandClass generalCommandClass)
                return generalCommandClass.AfterExecutionAsync(args.Context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return Task.CompletedTask;
    }

    private struct ChannelSpawnInfo
    {
        public int MessageCount;
        public DateTime LastTimeIncremented = DateTime.UtcNow.AddDays(-1);

        public ChannelSpawnInfo()
        {
        }
    }

    private struct CharacterExpGainInfo
    {
        public int MessageCount;
        public DateTime LastTimeIncremented = DateTime.UtcNow.AddDays(-1);

        public CharacterExpGainInfo()
        {
        }
    }
}