using System.Collections.Immutable;
using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands.Region;

public class Explore : GeneralCommandClass
{
    [Command("explore")]
    [Description("Use this command to encounter enemies while exploring a region, and get rewards for defeating them!")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask Execute(CommandContext ctx, string regionName = "")
    {
        var author = ctx.User;
        var userData = await DatabaseContext.Set<UserData>()
            .IncludeTeamWithAllEquipments()
            .Include(i => i.Items.Where(j => j is Stamina))
            .FirstOrDefaultAsync(i => i.DiscordId == ctx.User.Id);


        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }


        var embedToBuild = new DiscordEmbedBuilder()
            .WithUser(author)
            .WithTitle("Hmm")
            .WithColor(userData.Color)
            .WithDescription($"You cannot journey because you have not yet become an adventurer with {Tier.Unranked}");


        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(ctx);
            return;
        }

        if (userData.Tier == Tier.Unranked)
        {
            await ctx.RespondAsync(embedToBuild.Build());
            return;
        }

        var region = Region.GetRegion(regionName);
        if (region is null)
        {
            
            var regionString = $"These are the following existing regions:";
            if (regionName.Length > 0)
                regionString = $"Region with name `{regionName}` not found\n" + regionString;
            foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Region>())
                embedToBuild.AddField(i.Name,
                    $"Required Tier: **{i.TierRequirement}**\nRewards: **{i.WhatYouGain}**");
            embedToBuild.WithDescription(regionString);
            await ctx.RespondAsync(embedToBuild);
            return;
        }

        if (userData.Tier < region.TierRequirement)
        {
            embedToBuild.WithDescription(
                $"You need to be at least tier **{region.TierRequirement}** to explore **{region.Name}**");
            await ctx.RespondAsync(embedToBuild);
            return;
        }

        const int requiredStamina = 60;
        var stamina = userData.Items.OfType<Stamina>().FirstOrDefault();
        stamina?.RefreshEnergyValue();
        var staminaCount = stamina?.Stacks ?? 0;
        if (staminaCount < requiredStamina)
        {
            embedToBuild.WithDescription(
                $"You need at least {requiredStamina} stamina to explore. You have {staminaCount:N0} stamina");
            await ctx.RespondAsync(embedToBuild);
            return;
        }

        await MakeOccupiedAsync(userData);


        var groups = region.PossibleEnemies
            .Select(i => (Character)TypesFunction.GetDefaultObject(i))
            .GroupBy(i => i.Rarity)
            .ToImmutableArray();
        var characterType = BasicFunctions.RandomChoice(region.PossibleEnemies);

        var combatTier = userData.Tier;
        var enemyTeam = region.GenerateCharacterTeamFor(characterType, out var mainEnemyCharacter, combatTier);
        embedToBuild
            .WithTitle("Keep your guard up!")
            .WithDescription($"{mainEnemyCharacter.Name} has appeared!")
            .WithFooter($"Note: exploring costs {requiredStamina} stamina");
        await ctx.RespondAsync(embedToBuild);
        var message = await ctx.GetResponseAsync();
        await Task.Delay(2500);
        var userTeam = (PlayerTeam)userData.EquippedPlayerTeam!.LoadTeamStats();

        var simulator = new BattleSimulator(userTeam, enemyTeam);


        var battleResult = await simulator.StartAsync(message!);

        if (battleResult.Winners == userTeam)
        {
            var rewards = region.GetRewardsAfterBattle(mainEnemyCharacter, combatTier)
                .Append(new EntityReward([new Coin() { Stacks = 1000 * (int)combatTier }]))
                .ToArray();

         
            var rewardText = $"**You defeated {mainEnemyCharacter.Name}, and found some loot!**\n\n";
            rewardText += await userData.ReceiveRewardsAsync(DatabaseContext.Set<UserData>(), rewards);
            embedToBuild
                .WithTitle("Nice going bud!")
                .WithDescription($"**You won!**\n{rewardText}")
                .WithImageUrl("");
            stamina!.Stacks -= requiredStamina;
            await DatabaseContext.SaveChangesAsync();
            await message!.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embedToBuild));
        }
        else
        {
            var additionalString = "";
            if (battleResult.TimedOut is not null)
                additionalString += "timed out\n";
            if (battleResult.Forfeited is not null)
                additionalString += "forfeited";

            embedToBuild
                .WithTitle("Ah, too bad\n" + additionalString)
                .WithDescription("You lost boi");
            await message!.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embedToBuild));
        }
    }
}