using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.DialogueNamespace;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands;

public class Begin : GeneralCommandClass
{
    private static readonly DiscordTextInputComponent _askForName = new("What is your name?",
        "name", TypesFunction.GetDefaultObject<Player>().Name,
        TypesFunction.GetDefaultObject<Player>().Name, min_length: 3, max_length: 15);

    private static readonly DiscordButtonComponent _yes = new(DiscordButtonStyle.Primary, "yes", "Yes");
    private static readonly DiscordButtonComponent _no = new(DiscordButtonStyle.Primary, "no", "No");

    private static readonly DiscordTextInputComponent _askForGender = new("What's your gender?",
        "gender", Gender.Male.ToString(),
        Gender.Male.ToString());

    [Command("begin")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    [Description("Use this Commands to begin your journey")]
    public async ValueTask Execute(CommandContext ctx)
    {
        DiscordEmbedBuilder embedToBuild = new();
        var author = ctx.User;
        var userData = await DatabaseContext.Set<UserData>()
            .Where(i => i.DiscordId == ctx.User.Id)
            .FirstOrDefaultAsync();
        if (userData is null)
        {
            userData = await DatabaseContext.CreateNonExistantUserdataAsync(ctx.User.Id);
            await DatabaseContext.SaveChangesAsync();
        }

        if (userData.Tier > Tier.Unranked)
        {
            embedToBuild
                .WithTitle("Hmm")
                .WithDescription("`You have already begun`");
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder().AddEmbed(embedToBuild.Build()));
            return;
        }

        await DatabaseContext.Set<UserData>()
            .Include(i => i.Items.Where(j => j is Stamina))
            .Include(j => j.Characters)
            .ThenInclude(j => j.Blessing)
            .Include(i => i.Characters)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .Include(i => i.EquippedPlayerTeam)
            .ThenInclude(i => i!.TeamMemberships)
            .ThenInclude(i => i.Character)
            .Where(i => i.DiscordId == ctx.User.Id)
            .LoadAsync();
        var userColor = userData.Color;
        embedToBuild
            .WithUser(ctx.User)
            .WithColor(userColor);
        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(ctx);
            return;
        }
        

        await MakeOccupiedAsync(userData);
        embedToBuild.WithTitle($"{ctx.User.Username}, ")
            .WithDescription("Are you ready to embark on this journey?");
        await ctx.RespondAsync(new DiscordMessageBuilder()
            .AddEmbed(embedToBuild)
            .AddComponents([_yes, _no])
        );
        var message = (await ctx.GetResponseAsync())!;
        var interactionResult = await message.WaitForButtonAsync(ctx.User, new TimeSpan(0, 10, 0));
        if (interactionResult.TimedOut)
        {
            embedToBuild.WithDescription("Time out");
            await message.ModifyAsync(embedToBuild.Build());
            return;
        }

        if (interactionResult.Result.Id == "no")
        {
            embedToBuild.WithDescription("I see...");
            await interactionResult.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder().AddEmbed(embedToBuild));
            return;
        }

        var modalId = "begin_input_name_modal";
        await interactionResult.Result.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.Modal,
            new DiscordInteractionResponseBuilder()
                .WithTitle("Start of your journey, o powerful one")
                .AddComponents((IEnumerable<DiscordActionRowComponent>)
                    [new DiscordActionRowComponent([_askForName]), new DiscordActionRowComponent([_askForGender])])
                .WithCustomId(modalId));
        var done = await ctx.Client.ServiceProvider
            .GetService<InteractivityExtension>()!
            .WaitForModalAsync(modalId,
                ctx.User, new TimeSpan(0, 5, 0));
        if (done.TimedOut) return;
        userData.Name = done.Result.Values["name"];

        var interactionToRespondTo = done.Result.Interaction;
        if (!Enum.TryParse(done.Result.Values["gender"], true, out Gender gottenGender))
        {
            await interactionToRespondTo.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AsEphemeral()
                    .WithContent("You can only input male or female for gender"));
            return;
        }

        userData.Gender = gottenGender;
        var lily = new Lily();
        lily.Level = 5;
        if (!userData.Inventory.Any(i => i is Player))
        {
            Player player;
            if (gottenGender == Gender.Male)
            {
                player = new PlayerMale();
            }
            else
            {
                player = new PlayerFemale();
            }
            player.Level = 5;
            userData.Inventory.Add(player);
            player.UserData = userData;
            player.UserDataId = userData.Id;
        }

        var thePlayer = (Player)userData.Characters.First(i => i is Player);
        if (userData.EquippedPlayerTeam is null)
        {
            var playerTeam = new PlayerTeam();

            userData.EquippedPlayerTeam = playerTeam;
            userData.PlayerTeams.AddRange([
                playerTeam, new PlayerTeam { TeamName = "Team2" }, new PlayerTeam { TeamName = "Team3" },
                new PlayerTeam { TeamName = "Team4" }
            ]);

            playerTeam.UserDataId = userData.Id;
            playerTeam.UserData = userData;
            userData.EquippedPlayerTeam.Add(lily);


            foreach (var i in userData.PlayerTeams) i.Add(thePlayer);
        }

        if (!userData.EquippedPlayerTeam.Any()) userData.EquippedPlayerTeam.Add(thePlayer);
        var coachChad = new CoachChad();

        coachChad.Level = 5;
        var coachChadProfile = new DialogueProfile
        {
            CharacterColor = coachChad.Color, CharacterName = coachChad.Name,
            CharacterUrl = coachChad.ImageUrl
        };
        var lilyDialogueProfile = lily.DialogueProfile;

        DialogueNormalArgument[] dialogueArguments =
        [
            new DialogueNormalArgument
            {
                DialogueProfile = coachChadProfile,
                DialogueTexts =
                [
                    $"Hey {userData.Name}! my name is Chad. So you want to register as an adventurer? That's great!",
                    "But before you go on your adventure, I would need to confirm if you are strong enough to **Battle**!",
                    "Lily will accompany you just for this fight. When you feel like you have gotten the hang of **BATTLE**, click on **FORFEIT!**"
                ]
            },
            new DialogueNormalArgument
            {
                DialogueProfile = lilyDialogueProfile,
                DialogueTexts = [$"Let's give it our all {userData.Name}!"]
            }
        ];


        Dialogue theDialogue = new()
        {
            NormalArguments = dialogueArguments,

            Title = "Tutorial",
            RespondInteraction = true
        };
        var result = await theDialogue.LoadAsync(ctx.User, interactionToRespondTo);

        if (result.TimedOut)
        {
            theDialogue = new Dialogue
            {
                Title = "Beginning Of Journey",
                NormalArguments =
                [
                    new DialogueNormalArgument
                    {
                        DialogueProfile = coachChadProfile,
                        DialogueTexts =
                        [
                            "I can't believe you slept off..."
                        ]
                    }
                ],
                RemoveButtonsAtEnd = true
            };


            await theDialogue.LoadAsync(ctx.User, result.Message);
            return;
        }


        var userTeam = userData.EquippedPlayerTeam;

        coachChad.TotalMaxHealth = 900;
        coachChad.TotalDefense = 20;
        coachChad.TotalAttack = 1;
        coachChad.TotalSpeed = 100;
        var battleResult = await new BattleSimulator(userTeam.LoadTeamStats(),
            new NonPlayerTeam(characters: coachChad)).StartAsync(result.Message);

        if (battleResult.TimedOut is not null)
        {
            theDialogue = new Dialogue
            {
                Title = "Begin!",
                NormalArguments =
                [
                    new DialogueNormalArgument
                    {
                        DialogueProfile = coachChadProfile,
                        DialogueTexts = ["I can't believe you slept off during a battle..."]
                    }
                ],
                RemoveButtonsAtEnd = true
            };


            await theDialogue.LoadAsync(ctx.User, result.Message);
            return;
        }

        theDialogue = new Dialogue
        {
            NormalArguments =
            [
                new DialogueNormalArgument
                {
                    DialogueProfile = coachChadProfile,
                    DialogueTexts =
                    [
                        "Seems like you have gotten more used to battle.",
                        "You have completed the registration and you are now a **Bronze** tier adventurer! the lowest tier! you gotta work your way up the ranks!",
                        "I will see you later then! My coach job is done for today! let's go and report it, lily!"
                    ]
                },
                new DialogueNormalArgument
                {
                    DialogueProfile = lilyDialogueProfile,
                    DialogueTexts =
                    [
                        $"Actually, I want to journey with {userData.Name}. They seem interesting!",
                        "I hope you don't mind me quitting the job to journey with them"
                    ]
                },
                new DialogueNormalArgument
                {
                    DialogueProfile = coachChadProfile,
                    DialogueTexts =
                    [
                        "Very well, absolutely no problem there. I'll go report for you then",
                        $"Go have fun with {userData.Name}, lily! and see you later! ***i take my leave***"
                    ]
                },
                new DialogueNormalArgument
                {
                    DialogueProfile = lilyDialogueProfile,
                    DialogueTexts = ["Now, let's go!"]
                }
            ],
            RemoveButtonsAtEnd = false,
            RespondInteraction = false,
            Title = "Tutorial"
        };


        if (userData.Tier == Tier.Unranked)
        {
            userData.Tier = Tier.Bronze;
            userData.LastTimeQuestWasChecked = DateTime.UtcNow.AddDays(-1);
        }

        ;


        result = await theDialogue.LoadAsync(ctx.User, result.Message);
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
                        DialogueProfile = lilyDialogueProfile,
                        DialogueTexts =

                        [
                            "You slept off after becoming an adventurer... you are strange..."
                        ]
                    }
                ]
            };
            await theDialogue.LoadAsync(ctx.User, result.Message);
            return;
        }

        var rewardText = "";
        if (!userData.Characters.Any(i => i.GetType() == typeof(Lily)))
            rewardText = userData.ReceiveRewards(new EntityReward([lily]));
        message = result.Message;
        var stam = userData.Items.OfType<Stamina>().FirstOrDefault();
        if (stam is null)
        {
            stam = new Stamina();
            stam.RefreshEnergyValue();
            stam.Stacks = stam.MaxEnergyValue;
            userData.Items.Add(stam);
        }
        await DatabaseContext.SaveChangesAsync();
        await message.ModifyAsync(new DiscordMessageBuilder()
            .AddEmbed(embedToBuild.WithTitle("Nice!").WithUser(ctx.User).WithDescription(rewardText)
            ));
    }
}