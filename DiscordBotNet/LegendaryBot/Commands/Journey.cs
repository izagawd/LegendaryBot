using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Commands;

public class Journey : GeneralCommandClass
{
    

    [Command("journey"), Description("Use this command to fight any random 3 star, and optionto get them if you beat them"),
    AdditionalCommand("/hunt CoachChad",BotCommandType.Battle)]
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

        var characterType = BasicFunctionality.RandomChoice(TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Character>()
            .Where(i => i.Rarity <= Rarity.ThreeStar)).GetType();
 
        await MakeOccupiedAsync(userData);
        var enemyTeam = new CharacterTeam();
        
        enemyTeam.Add((Character)Activator.CreateInstance(characterType)!);
        enemyTeam.First().Level = 5 + (((int)userData.Tier-1) * 10);
        enemyTeam.LoadTeamStats();
        
      

        embedToBuild = embedToBuild
            .WithTitle($"Keep your guard up!")
            .WithDescription($"{enemyTeam.First().Name}(s) have appeared!");
        await ctx.RespondAsync(embedToBuild.Build());
        var message =  await ctx.GetResponseAsync();
        await Task.Delay(2500);
        var userTeam = userData.EquippedPlayerTeam!.LoadTeamStats();

        var simulator = new BattleSimulator(userTeam,  enemyTeam.LoadTeamStats());

 

        var battleResult = await simulator.StartAsync(message!);



        var expToGain = Character.GetExpBasedOnDefeatedCharacters(enemyTeam);
        var coinsToGain = Character.GetCoinsBasedOnCharacters(enemyTeam);
        if (battleResult.Winners != userTeam)
        {
            expToGain = (expToGain/5)+1;
     
        }
      
        
        var expGainText = userTeam.IncreaseExp(expToGain);
        
           
        if (battleResult.Winners == userTeam)
        {
            var rewardText = userData.ReceiveRewards([new CoinsReward(coinsToGain),
                ..enemyTeam.SelectMany(i => i.DroppedRewards)]);
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
                .WithDescription($"You lost boi\n"+expGainText);
            await message!.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embedToBuild));
            
        }

        await DatabaseContext.SaveChangesAsync();

    }
}