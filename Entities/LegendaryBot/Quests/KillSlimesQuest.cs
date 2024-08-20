using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities.LegendaryBot.Quests;

public class KillSlimesQuest : Quest
{
    public override int TypeId
    {
        get => 1;
        protected init { }
    }


    public override string Title => "Kill Slimes";
    public override string Description => "Simple quest. Defeat the slimes that are attacking a village";

    public override IEnumerable<Reward> QuestRewards { get; protected set; } = [];

    public override async Task<bool> StartQuest(IQueryable<UserData> userDataQueryable, CommandContext context,
        DiscordMessage message)
    {
        var slimeTeam = new CharacterTeam([new Slime(), new Slime()]);

        var userData = await userDataQueryable
            .IncludeTeamWithAllEquipments()
            .FirstAsync(i => i.Id == UserDataId);
        foreach (var i in slimeTeam) i.SetBotStatsAndLevelBasedOnTier(userData.Tier);
        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Watch out!")
            .WithDescription("A bunch of slimes are attacking!");
        if (message is null)
            message = await context.Channel.SendMessageAsync(embed);
        else
            message = await message.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
        await Task.Delay(2000);
        var playerTeam = userData.EquippedPlayerTeam.LoadTeamStats();

        var battleSimulator = new BattleSimulator(playerTeam, slimeTeam);
        var result = await battleSimulator.StartAsync(message);


        if (result.Winners == playerTeam)
        {
            var str = playerTeam.IncreaseExp(Character.GetExpBasedOnDefeatedCharacters(slimeTeam));
            QuestRewards = [new TextReward(str)];
        }

        return result.Winners == playerTeam;
    }
}