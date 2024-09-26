using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities.LegendaryBot.Quests;

public class SkeletonAttack : Quest
{
    public override int TypeId
    {
        get => 4;
        protected init{}
    }
    public override string Description => "Defeat many skeletons";
    public override string Title => "Skeleton Attack";
    public override IEnumerable<Reward>? QuestRewards { get; protected set; }
  
    public override async Task<bool> StartQuest(IQueryable<UserData> userDataQueryable, CommandContext context,
        DiscordMessage message)
    {
        var slimeTeam = new NonPlayerTeam([new Slime(), new Slime()]);

        var userData = await userDataQueryable
            .IncludeTeamWithAllEquipments()
            .FirstAsync(i => i.Id == UserDataId);
        foreach (var i in slimeTeam) i.SetBotStatsAndLevelBasedOnTier(userData.Tier);
        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Watch out!")
            .WithDescription("Waves of skeletons are attacking!");
        if (message is null)
            message = await context.Channel.SendMessageAsync(embed);
        else
            message = await message.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
        var waves = 2;
        if (userData.Tier > Tier.Silver)
            waves = 3;

        BattleResult battleResult = null!;
        userData.EquippedPlayerTeam!.LoadTeamStats().SetEveryoneToMaxHealth();
        await Task.Delay(2000);
        var simu = new BattleSimulator();
        simu.Team1 = userData.EquippedPlayerTeam;
        foreach (var i in Enumerable.Range(0,waves))
        {
            NonPlayerTeam skeleTeam = [];
            var count = 2;
            if (userData.Tier >= Tier.Platinum)
                count = 4;
            else if (userData.Tier >= Tier.Silver)
                count = 3;
            foreach (var j in Enumerable.Range(0,count))
            {
                var skele = new Skeleton();
                skele.SetBotStatsAndLevelBasedOnTier(userData.Tier);
                skeleTeam.Add(skele);
            }

            simu.Team2 = skeleTeam.LoadTeamStats();
          battleResult = await simu.StartAsync(message);
            if (battleResult.Winners != userData.EquippedPlayerTeam)
                break;
            if (i != waves - 1)
            {
                await message.ModifyAsync(new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithUser(context.User)
                        .WithColor(userData.Color)
                        .WithTitle("Don't rest now!")
                        .WithDescription("there's more skeletons!")
                        .Build()));
                await Task.Delay(3000);
            }
        }
     

        return battleResult.Winners == userData.EquippedPlayerTeam;
    }
    
}