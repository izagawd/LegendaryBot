using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.DialogueNamespace;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;


namespace DiscordBotNet.LegendaryBot.command;

public class Begin : GeneralCommandClass
{


    private static TextInputComponent _askForName = new("What is your name?",
        "name", DefaultObjects.GetDefaultObject<Player>().Name,
        DefaultObjects.GetDefaultObject<Player>().Name, min_length: 3, max_length: 15);
    
    
    private static TextInputComponent _askForGender = new("What's your gender?",
        "gender", Gender.Male.ToString(),
        Gender.Male.ToString());
    [SlashCommand("begin", "Begin your journey by playing the tutorial!"),
    AdditionalSlashCommand("/begin",BotCommandType.Battle)]
    public async Task Execute(InteractionContext ctx)
    {
        DiscordEmbedBuilder embedToBuild = new();
        DiscordUser author = ctx.User;

        UserData userData = await DatabaseContext.UserData
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Blessing)
            .Include(i => i.Inventory)
            .ThenInclude(i => (i as Character).Gears)
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Inventory.Where(j => j is Character))
            .FindOrCreateUserDataAsync((long)author.Id);

        DiscordColor userColor = userData.Color;
        if (userData.IsOccupied)
        {
           
            embedToBuild
                .WithTitle("Hmm")
                .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
                .WithDescription("`You are occupied`")
                .WithColor(userColor);

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embedToBuild.Build()));
            return;
        }


        if (userData.Tier != Tier.Unranked)
        {
            
            embedToBuild
                .WithTitle("Hmm")
                .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
                .WithDescription("`You have already begun`")
                .WithColor(userColor);

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embedToBuild.Build()));
            return;
        }
    
        await MakeOccupiedAsync(userData);
        var modalId = "begin_input_name_modal";
        await ctx.CreateResponseAsync(InteractionResponseType.Modal,
            new DiscordInteractionResponseBuilder()
                .WithTitle("Start of your journey, o powerful one")
                .AddComponents( (IEnumerable<DiscordActionRowComponent>) 
                    [new DiscordActionRowComponent([_askForName]),new DiscordActionRowComponent([_askForGender])])
                .WithCustomId(modalId));
        var done = await ctx.Client.GetInteractivity()
            .WaitForModalAsync(modalId,
            ctx.User, new TimeSpan(0, 5, 0));
        if (done.TimedOut)
        {
            return;
        }
        userData.Name = done.Result.Values["name"];
        userData.Name.Print();
        var interactionToRespondTo = done.Result.Interaction;
        if (!Enum.TryParse(done.Result.Values["gender"], true, out Gender gottenGender))
        {
            await interactionToRespondTo.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("You can only input male or female for gender"));
            return;
        }
        userData.Gender = gottenGender;
        var lily = new Lily();
        
        if (!userData.Inventory.Any(i => i is Player))
        {
            Player player = new Player();

            player.SetElement(Element.Fire);
            userData.Inventory.Add(player);
            player.UserData = userData;
            player.UserDataId = userData.Id;
        }
        var thePlayer = userData.Inventory.OfType<Player>().First();
        if (userData.EquippedPlayerTeam is null)
        {
            var playerTeam = new PlayerTeam();
            
            userData.EquippedPlayerTeam = playerTeam;
            userData.PlayerTeams.AddRange([playerTeam, new PlayerTeam(){TeamName = "Team2"},new PlayerTeam(){TeamName = "Team3"},new PlayerTeam(){TeamName = "Team4"}]);
        
            playerTeam.UserDataId = userData.Id;
            
            

            foreach (var i in userData.PlayerTeams)
            {
                i.Add(thePlayer);
            }
        }

        if (!userData.EquippedPlayerTeam.Any())
        {
            userData.EquippedPlayerTeam.Add(thePlayer);
        }
        var coachChad = new CoachChad();
    
        coachChad.SetLevel(100);
        var coachChadProfile = new DialogueProfile
        {
            CharacterColor = coachChad.Color, CharacterName = coachChad.Name,
            CharacterUrl = coachChad.ImageUrl,
        };
        
        
        DialogueNormalArgument[] dialogueArguments =
        [
            new()
            {
                DialogueProfile = coachChadProfile,
                DialogueTexts =
                    [
                        $"Hey {userData.Name}! my name is Chad. So you want to register as an adventurer? That's great!",
                        "But before you go on your adventure, I would need to confirm if you are strong enough to **Battle**!",
                        "Lily will accompany you just for this fight. When you feel like you have gotten the hang of **BATTLE**, click on **FORFEIT!**"
                    ]
            },
            new()
            {
                DialogueProfile = lily.DialogueProfile,
                DialogueTexts = [$"Let's give it our all {userData.Name}!"]
            }
        ];



        Dialogue theDialogue = new()
        {
            NormalArguments = dialogueArguments,

            Title = "Tutorial",
            RespondInteraction = true,

        };
        DialogueResult result = await theDialogue.LoadAsync(interactionToRespondTo);

        if (result.TimedOut)
        {
            theDialogue = new Dialogue()
            {
                Title = "Beginning Of Journey",
                NormalArguments =
                [
                    new DialogueNormalArgument
                    {
                        DialogueProfile = coachChadProfile,
                        DialogueTexts = [
                            "I can't believe you slept off..."
                        ],
                    }
                ],
                RemoveButtonsAtEnd = true
            };


            await theDialogue.LoadAsync(ctx.Interaction,result.Message);
            return;
        }


        var userTeam = userData.EquippedPlayerTeam;
        userTeam.Add(lily);
        coachChad.TotalMaxHealth = 9000;
        coachChad.TotalDefense = 100;
        coachChad.TotalAttack = 1;
        coachChad.TotalSpeed = 100;
        BattleResult battleResult = await new BattleSimulator(userTeam.LoadTeamEquipment(), 
            new CharacterTeam(characters: coachChad).LoadTeamEquipment()).StartAsync(result.Message);

        if (battleResult.TimedOut is not null)
        {
            theDialogue = new Dialogue
            {
                Title = "Begin!",
                NormalArguments =
                [
                    new()
                    {
                        DialogueProfile = coachChadProfile,
                        DialogueTexts = ["I can't believe you slept off during a battle..."]
                    }
                ],
                RemoveButtonsAtEnd = true
            };
    
            

            await theDialogue.LoadAsync(ctx, result.Message);
            return;
        }

        theDialogue = new Dialogue()
        {

            NormalArguments =        [
                new ()
                {
                    DialogueProfile = coachChadProfile,
                    DialogueTexts =
                    [
                        "Seems like you have gotten more used to battle.",
                        "You have completed the registration and you are now a **Bronze** tier adventurer! the lowest tier! you gotta work your way up the ranks!",
                        "I will see you later then! I have other new adventurers to attend to! Let's go attend to them Lily!"
                    ]
                }
            ],
            RemoveButtonsAtEnd = true, 
            RespondInteraction = false,
            Title = "Tutorial"
        };


        userData.Tier = await DatabaseContext.UserData.FindOrCreateSelectUserDataAsync((long)author.Id, i => i.Tier);

        if (userData.Tier == Tier.Unranked)
        {
            userData.Tier = Tier.Bronze;
            userData.LastTimeQuestWasChecked = DateTime.UtcNow.AddDays(-1);
        };
        userTeam.Remove(lily);

        await DatabaseContext.SaveChangesAsync();
      

      
        result =  await theDialogue.LoadAsync(ctx, result.Message);
        if (result.TimedOut)
        {
            theDialogue = new Dialogue
            {
                Title = "Beginning of journey",
                RemoveButtonsAtEnd = true,
                NormalArguments =
                [
                    new DialogueNormalArgument
                    {
                        DialogueProfile = coachChadProfile,
                        DialogueTexts =

                        [
                            "You slept off after becoming an adventurer... you are strange..."
                        ],
   
                    }
                ]
            };
                

            await theDialogue.LoadAsync(ctx, result.Message);
        }
        if (result.Skipped)
        {
            await result.Message.ModifyAsync(
                new DiscordMessageBuilder()
                    .WithEmbed(result.Message.Embeds.First()));
        }

    }
}