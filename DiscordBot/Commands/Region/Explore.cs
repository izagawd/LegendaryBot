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
    [Description("Use this command to encounter a character while exploring a region, and get them when you beat them")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask Execute(CommandContext ctx, string regionName)
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
            var regionString = $"Region with name `{regionName}` not found\nThese are the following existing regions:";
            foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Region>())
                embedToBuild.AddField(i.Name,
                    $"Required Tier: **{i.TierRequirement}**");
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

        await MakeOccupiedAsync(userData);

        var groups = region.ObtainableCharacters
            .Select(i => (Character)TypesFunction.GetDefaultObject(i))
            .GroupBy(i => i.Rarity)
            .ToImmutableArray();
        var characterGrouping = BasicFunctions.GetRandom(new Dictionary<IGrouping<Rarity, Character>, double>
        {
            { groups.First(i => i.Key == Rarity.ThreeStar), 85 },
            { groups.First(i => i.Key == Rarity.FourStar), 14 },
            { groups.First(i => i.Key == Rarity.FiveStar), 1 }
        });
        var characterType
            = BasicFunctions.RandomChoice(characterGrouping.Select(i => i)).GetType();

        var enemyTeam = region.GenerateCharacterTeamFor(characterType, out var character);
        embedToBuild
            .WithTitle("Keep your guard up!")
            .WithDescription($"{enemyTeam.First().Name}(s) have appeared!");
        await ctx.RespondAsync(embedToBuild);
        var message = await ctx.GetResponseAsync();
        await Task.Delay(2500);
        var userTeam =(PlayerTeam) userData.EquippedPlayerTeam!.LoadTeamStats();

        var simulator = new BattleSimulator(userTeam, enemyTeam);


        var battleResult = await simulator.StartAsync(message!);


        var expToGain = Character.GetExpBasedOnDefeatedCharacters(enemyTeam);
        var coinsToGain = Character.GetCoinsBasedOnCharacters(enemyTeam);
        if (battleResult.Winners != userTeam) expToGain = expToGain / 5;


        var expGainText = userTeam.IncreaseExp(expToGain);
        if (battleResult.Winners == userTeam)
        {
            character.Level = 1;
            List<Reward> rewards =
            [
                new EntityReward([character, new Coin { Stacks = coinsToGain }]),
                ..enemyTeam.SelectMany(i => i.DroppedRewards), new UserExperienceReward(250)
            ];
            if (character.Blessing is not null) rewards.Add(new EntityReward([character.Blessing]));

            var otherChar = enemyTeam
                .Where(i => i != character)
                .MinBy(_ => BasicFunctions.GetRandomNumberInBetween(0, 100));
            if (otherChar is not null)
            {
                otherChar.Level = 1;
                rewards.Add(new EntityReward([otherChar]));
            }


            var rewardText = userData.ReceiveRewards(rewards);

            rewardText += $"\nYou explored a bit more after recruiting {character.Name}\n";

            rewards.Clear();


            var expMatCount = BasicFunctions.GetRandomNumberInBetween(3, 5);
            var gearCount = BasicFunctions.GetRandomNumberInBetween(1, 2);

            foreach (var _ in Enumerable.Range(0, gearCount))
            {
                var gear
                    = (Gear)Activator.CreateInstance(BasicFunctions.RandomChoice(Gear.AllGearTypes))!;
                var gearRarityToGive = BasicFunctions.RandomChoice([Rarity.OneStar, Rarity.TwoStar]);
                gear.Initialize(gearRarityToGive);
                rewards.Add(new EntityReward([gear]));
            }

            foreach (var _ in Enumerable.Range(0, expMatCount))
                rewards.Add(new EntityReward([new HerosKnowledge { Stacks = 1 }]));
            rewards.Add(new EntityReward([new Coin { Stacks = 5000 }]));
            await DatabaseContext.Set<Item>().Where(i => i is HerosKnowledge || i is Coin)
                .LoadAsync();
            rewardText += userData.ReceiveRewards(rewards);
            embedToBuild
                .WithTitle("Nice going bud!")
                .WithDescription($"You won!\n{expGainText}\n{rewardText}")
                .WithImageUrl("");

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
                .WithDescription($"You lost boi\n{expGainText}");
            await message!.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embedToBuild));
        }

        await DatabaseContext.SaveChangesAsync();
    }
}