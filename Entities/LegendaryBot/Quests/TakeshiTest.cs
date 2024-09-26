using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.DialogueNamespace;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities.LegendaryBot.Quests;

public class TakeshiTest : Quest
{
    public override int TypeId
    {
        get => 3; protected init{} }

    public override string Description => "Takeshi Combat Test";
    public override string Title => "Takeshi Combat Test";
    public override async Task<QuestResult> StartQuest(IQueryable<UserData> userDataQueryable, CommandContext context,
        DiscordMessage messageToEdit)
    {
        var userData = await userDataQueryable
            .IncludeTeamWithAllEquipments()
            .FirstOrDefaultAsync(i => i.DiscordId == context.User.Id);
        var takeshi = new Takeshi();
        takeshi.SetBotStatsAndLevelBasedOnTier(userData!.Tier);
        NonPlayerTeam takeshiTeam = [takeshi];
        var dialogue = new Dialogue
        {
            Title = Title,
            NormalArguments = [new DialogueNormalArgument
            {
                DialogueProfile = takeshi.DialogueProfile,
                DialogueTexts = ["I shall test your strength!"]
            }]
        };
        var result =await dialogue.LoadAsync(context.User, messageToEdit);
        if (result.TimedOut)
        {
            return QuestResult.Fail();
        }

        var battle = new BattleSimulator(userData.EquippedPlayerTeam.LoadTeamStats()
                .SetEveryoneToMaxHealth(),
            takeshiTeam.SetEveryoneToMaxHealth());
        var battleResult = await battle.StartAsync(result.Message);
        if (battleResult.Winners == userData.EquippedPlayerTeam)
        {
            dialogue = new Dialogue
            {
                Title = Title,
                NormalArguments = [new DialogueNormalArgument
                {
                    DialogueProfile = takeshi.DialogueProfile,
                    DialogueTexts = ["Good. You are strong"]
                }]
            };
            await dialogue.LoadAsync(context.User, battleResult.Message);
            return QuestResult.Success();
        }
        dialogue = new Dialogue
        {
            Title = Title,
            NormalArguments = [new DialogueNormalArgument
            {
                DialogueProfile = takeshi.DialogueProfile,
                DialogueTexts = ["You have gotten rustier","Train more, then come fight me again"]
            }]
        };
        await dialogue.LoadAsync(context.User, battleResult.Message);
        return QuestResult.Fail();
    }
}