using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.DialogueNamespace;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.Quests;

public class DirectionHelping : Quest
{
    public override string Description => "You are tasked with giving people directions";
    public override async Task<bool> StartQuest(PostgreSqlContext databaseContext, CommandContext context,
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

        var userData = await databaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateUserDataAsync(context.User.Id);
        if (dialogueResult.Decision == "right")
        {
            dialogue = new Dialogue()
            {
                Title = "directions",

                NormalArguments =
                [
                    new DialogueNormalArgument()
                    {
                        DialogueProfile = profile,
                        DialogueTexts = [$"Guess I don't have to make an explosion out of you",
                        $"I knew the direction, I just wanted to see if {userData.Tier} tiers were smart",
                        $"Get stronger so trying to detonate you will be more interesting, {userData.Name}."]
                    }
                ]
            };
            await dialogue.LoadAsync(context.User, dialogueResult.Message);
            return true;
        }
        dialogue = new Dialogue()
        {
            Title = "directions",

            NormalArguments =
            [
                new DialogueNormalArgument()
                {
                    DialogueProfile = profile,
                    DialogueTexts = ["Wrong... you are so dumb, guess i'll detonate you"]
                }
            ]
        };

        dialogueResult = await dialogue.LoadAsync(context.User, dialogueResult.Message);
        var blastTeam = new CharacterTeam(blast);

        blast.Level =60;
        blast.TotalAttack = 2500;
        blast.TotalSpeed = 150;
        blast.TotalMaxHealth = 50000;
        blast.TotalCriticalChance = 80;
        blast.TotalCriticalDamage = 300;
        blast.TotalDefense = 750;
        blast.TotalMaxHealth = 40000;
        blast.Health = 40000;
        

        var userTeam = userData.EquippedPlayerTeam;
        userTeam.LoadTeamEquipment();
        var battle = new BattleSimulator(userTeam,blastTeam);

        var battleResult = await battle.StartAsync(dialogueResult.Message);

        if (battleResult.Winners == userTeam)
        {
            dialogue = new Dialogue()
            {
                Title = "direction",
                NormalArguments =
                [
                    new DialogueNormalArgument()
                    {
                        DialogueProfile = profile,
                        DialogueTexts =
                        [
                            "How... did you win...?",
                            "You are a bronze tier... you shouldn't be able to beat me...",
                            "h-how..."
                        ]
                    }
                ]
            };
             await dialogue.LoadAsync(context.User, battleResult.Message);
            QuestRewards = [new TextReward(userTeam.IncreaseExp(Character.GetExpBasedOnDefeatedCharacters(blastTeam))),
                    new CoinsReward(Character.GetCoinsBasedOnCharacters(blastTeam))];
            
            return true;
        }

        dialogue = new Dialogue()
        {
            Title = "direction",
            NormalArguments =
            [
                new DialogueNormalArgument()
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

    public override IEnumerable<Reward> QuestRewards { get; protected set; } = [];
}