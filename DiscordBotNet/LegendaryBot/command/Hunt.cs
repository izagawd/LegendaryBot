using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using DSharpPlus.Commands;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.command;

public class Hunt : GeneralCommandClass
{
    

    [Command("hunt"),
    AdditionalCommand("/hunt Coach Chad",BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx,
        [Parameter("mob_name")] string characterName,
        [Parameter("enemy_count")] long enemyCount = 1 )
    {
        var author = ctx.User;
        var userData = await DatabaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateUserDataAsync(author.Id);


        var embedToBuild = new DiscordEmbedBuilder()
            .WithUser(author)
            .WithTitle("Hmm")
            .WithColor(userData.Color)
            .WithDescription($"You cannot hunt because you have not yet become an adventurer with {Tier.Unranked}");
        if (enemyCount < 1)
        {
            embedToBuild.WithDescription("There must be at least one enemy");
            await ctx.RespondAsync(embedToBuild);
            return;
        }

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
        
        var characterType = DefaultObjects.AllAssemblyTypes.FirstOrDefault(
            i =>  i.Name.ToLower() == characterName.ToLower().Replace(" ", "") && i.IsSubclassOf(typeof(Character)) && !i.IsRelatedToType(typeof(Player)));
        if (characterType is null)
        {
            embedToBuild =
                embedToBuild.WithDescription($"Mob {characterName} does not exist!");
            await ctx.RespondAsync(embedToBuild.Build());
            return;
        }

        await MakeOccupiedAsync(userData);
        var enemyTeam = new CharacterTeam();
        foreach (var _ in Enumerable.Range(0,(int) enemyCount))
        {
            enemyTeam.Add((Character)Activator.CreateInstance(characterType)!);
        }
      

        embedToBuild = embedToBuild
            .WithTitle($"Keep your guard up!")
            .WithDescription($"Wild {enemyTeam.First()}(s) have appeared!");
        await ctx.RespondAsync(embedToBuild.Build());
        var message =  await ctx.GetResponseAsync();
        var userTeam = userData.EquippedPlayerTeam!.LoadTeamEquipment();
        foreach (var i in enemyTeam)
        {
            i.SetLevel(userTeam.Select(j => j.Level).Average().Round());
        }

        var simulator = new BattleSimulator(userTeam,  enemyTeam.LoadTeamEquipment());

 

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
            var rewardText = userData.ReceiveRewards(author.Username, [new CoinsReward(coinsToGain),
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
                .WithDescription($"You lost boii\n"+expGainText);
            await message!.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embedToBuild));
            
        }

        await DatabaseContext.SaveChangesAsync();

    }
}