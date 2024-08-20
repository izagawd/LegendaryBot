using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.DialogueNamespace;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities.LegendaryBot.Quests;

public class DirectionHelping : Quest
{
    public override int TypeId
    {
        get => 2;
        protected init { }
    }


    public override string Title => "Direction Helping";
    public override string Description => "You are tasked with giving people directions";

    public override IEnumerable<Reward> QuestRewards { get; protected set; } = [];

    public override async Task<bool> StartQuest(IQueryable<UserData> userDataQueryable, CommandContext context,
        DiscordMessage messageToEdit)
    {
        var blast = new Blast();
        var profile = blast.DialogueProfile;
        var decisionArgument = new DialogueDecisionArgument
        {
            DialogueProfile = profile,
            ActionRows =
            [
                new DiscordActionRowComponent([
                    new DiscordButtonComponent(DiscordButtonStyle.Primary,
                        "right",
                        "(Point Right)"),
                    new DiscordButtonComponent(DiscordButtonStyle.Danger,
                        "left",
                        "(Point Left)")
                ])
            ],
            DialogueText
                = "Yo, human, do you know where I can find the nearest restaurant? I think it's right but I'm not sure"
        };
        var dialogue = new Dialogue
        {
            Title = "directions",
            DecisionArgument = decisionArgument,
            Skippable = false
        };


        var dialogueResult = await dialogue.LoadAsync(context.User, messageToEdit);

        var userData = await userDataQueryable
            .IncludeTeamWithAllEquipments()
            .FirstAsync(i => i.DiscordId == context.User.Id);

        if (dialogueResult.Decision == "right")
        {
            dialogue = new Dialogue
            {
                Title = "directions",

                NormalArguments =
                [
                    new DialogueNormalArgument
                    {
                        DialogueProfile = profile,
                        DialogueTexts =
                        [
                            "Guess I don't have to make an explosion out of you",
                            $"I knew the direction, I just wanted to see if {userData.Tier} tiers were smart",
                            $"Get stronger so trying to detonate you will be more interesting, {userData.Name}."
                        ]
                    }
                ]
            };
            await dialogue.LoadAsync(context.User, dialogueResult.Message);
            return true;
        }

        dialogue = new Dialogue
        {
            Title = "directions",

            NormalArguments =
            [
                new DialogueNormalArgument
                {
                    DialogueProfile = profile,
                    DialogueTexts = ["Wrong... you are so dumb, guess i'll detonate you"]
                }
            ]
        };

        dialogueResult = await dialogue.LoadAsync(context.User, dialogueResult.Message);
        var blastTeam = new CharacterTeam(blast);

        blast.SetBotStatsAndLevelBasedOnTier(Tier.Divine);

        var userTeam = userData.EquippedPlayerTeam;
        userTeam.LoadTeamStats();
        var battle = new BattleSimulator(userTeam, blastTeam);

        var battleResult = await battle.StartAsync(dialogueResult.Message);

        if (battleResult.Winners == userTeam)
        {
            dialogue = new Dialogue
            {
                Title = "direction",
                NormalArguments =
                [
                    new DialogueNormalArgument
                    {
                        DialogueProfile = profile,
                        DialogueTexts =
                        [
                            "How... did you win...?",
                            "h-how..."
                        ]
                    }
                ]
            };

            await dialogue.LoadAsync(context.User, battleResult.Message);
            await userDataQueryable
                .Where(i => i.Id == userData.Id)
                .Select(i => i.Items.Where(j => j is Coin))
                .LoadAsync();
            QuestRewards =
            [
                new TextReward(userTeam.IncreaseExp(Character.GetExpBasedOnDefeatedCharacters(blastTeam))),
                new EntityReward([new Coin { Stacks = Character.GetCoinsBasedOnCharacters(blastTeam) }])
            ];

            return true;
        }

        dialogue = new Dialogue
        {
            Title = "direction",
            NormalArguments =
            [
                new DialogueNormalArgument
                {
                    DialogueProfile = profile,
                    DialogueTexts =
                    [
                        "Hmph, weak"
                    ]
                }
            ]
        };
        await dialogue.LoadAsync(context.User, battleResult.Message);


        return false;
    }
}