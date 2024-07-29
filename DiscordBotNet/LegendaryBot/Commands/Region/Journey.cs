using System.Collections.Immutable;
using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Journey : GeneralCommandClass
{
    
    static Journey()
    {
        
    }
    

    [Command("journey"), Description("Use this command to encounter a character, and get them if you beat them"),
    BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var author = ctx.User;
        var userData = await DatabaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FirstOrDefaultAsync(i => i.Id == ctx.User.Id);
        
    

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
            embedToBuild
                .WithDescription("You are occupied!");
            await ctx.RespondAsync(embedToBuild);
            return;
        }

        if (userData.Tier == Tier.Unranked)
        {
            await ctx.RespondAsync(embedToBuild.Build());
            return;
        }
        userData.RefreshEnergyValue();
        const int requiredEnergy = 40;
        if (userData.EnergyValue < requiredEnergy)
        {
            embedToBuild.WithDescription($"You need at least {requiredEnergy} energy to journey!");
            await ctx.RespondAsync(embedToBuild);
            return;
        }

        var yes = new DiscordButtonComponent(DiscordButtonStyle.Success,
            "yes", "yes");
        var no = new DiscordButtonComponent(DiscordButtonStyle.Success,
            "no", "no");



        await MakeOccupiedAsync(userData);
        embedToBuild
            .WithTitle(userData.Name)
            .WithDescription($"{requiredEnergy} energy will be consumed. Proceed?");
        var messageBuilder = new DiscordMessageBuilder()
            .AddComponents(yes, no)
            .AddEmbed(embedToBuild);
        await ctx.RespondAsync(messageBuilder);

        var message = await ctx.GetResponseAsync();
        var result = await message.WaitForButtonAsync();

        if (result.TimedOut || result.Result.Id != "yes")
        {
            yes.Disable();
            no.Disable();
            if (result.TimedOut)
            {
                await message.ModifyAsync(messageBuilder);
            }
            else
            {
                await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder(messageBuilder));
            }
            return;
        }

        var groups = TypesFunctionality
            .GetDefaultObjectsThatIsInstanceOf<Character>()
            .Where(i => i.CanSpawnNormally)
            .GroupBy(i => i.Rarity)
            .ToImmutableArray();
        
        
        var characterGrouping= BasicFunctionality.GetRandom(new Dictionary<IGrouping<Rarity, Character>, double>()
        {
            {groups.First(i => i.Key == Rarity.ThreeStar),85},
            {groups.First(i => i.Key == Rarity.FourStar), 14},
            {groups.First(i => i.Key == Rarity.FiveStar),1}
        });
        var characterType 
            = BasicFunctionality.RandomChoice(characterGrouping.Select(i => i)).GetType();
   
        var enemyTeam = new CharacterTeam();

        var character = (Character)Activator.CreateInstance(characterType)!;
        enemyTeam.Add(character);
        character.SetBotStatsAndLevelBasedOnTier(userData.Tier);
        embedToBuild
            .WithTitle($"Keep your guard up!")
            .WithDescription($"{enemyTeam.First().Name}(s) have appeared!");
        await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().AddEmbed(embedToBuild));

        await Task.Delay(2500);
        var userTeam = userData.EquippedPlayerTeam!.LoadTeamStats();

        var simulator = new BattleSimulator(userTeam,  enemyTeam);

 

        var battleResult = await simulator.StartAsync(message!);



        var expToGain = Character.GetExpBasedOnDefeatedCharacters(enemyTeam);
        var coinsToGain = Character.GetCoinsBasedOnCharacters(enemyTeam);
        if (battleResult.Winners != userTeam)
        {
            expToGain = expToGain/5;
        }


        var expGainText = userTeam.IncreaseExp(expToGain);
        userData.EnergyValue -= requiredEnergy;
        if (battleResult.Winners == userTeam)
        {

            character.Level = 1;
            var rewardText = userData.ReceiveRewards([ new EntityReward([character]),new CoinsReward(coinsToGain),
                ..enemyTeam.SelectMany(i => i.DroppedRewards), new UserExperienceReward(250),
           ]);
           
            rewardText += $"\nYou explored a bit more after recruiting {character.Name}\n";
   
            List<Reward> rewards = [];
           
 
            var expMatCount = BasicFunctionality.GetRandomNumberInBetween(3, 5);
            var gearCount = BasicFunctionality.GetRandomNumberInBetween(1, 2);
            
            foreach (var _ in Enumerable.Range(0,gearCount))
            {
                var gear
                    = (Gear) Activator.CreateInstance(BasicFunctionality.RandomChoice(Gear.AllGearTypes))!;
                var gearRarityToGive = BasicFunctionality.RandomChoice([Rarity.OneStar, Rarity.TwoStar]);
                gear.Initialize(gearRarityToGive);
                rewards.Add(new EntityReward([gear]));
            }
            foreach (var _ in Enumerable.Range(0, expMatCount))
            {
                rewards.Add(new EntityReward([new HerosKnowledge()]));
            }
            rewards.Add(new CoinsReward(5000));
            await DatabaseContext.Items.Where(i => i is HerosKnowledge)
                .LoadAsync();
            rewardText += userData.ReceiveRewards(rewards);
            embedToBuild
                .WithTitle($"Nice going bud!")
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
                .WithTitle($"Ah, too bad\n"+additionalString)
                .WithDescription($"You lost boi\n{expGainText}");
            await message!.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embedToBuild));
            
        }
        
        await DatabaseContext.SaveChangesAsync();

    }
}