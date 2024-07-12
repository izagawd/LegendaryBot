using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.command;

public class Hunt : GeneralCommandClass
{
    

    [SlashCommand("hunt", "Hunt for mobs to get materials and be stronger!"),
    AdditionalSlashCommand("/hunt Coach Chad",BotCommandType.Battle)]
    public async Task Execute(InteractionContext ctx,
        [Option("mob_name", "The name of the mob you want to hunt")] string characterName,
        [Option("enemy_count","number of enemies")] long enemyCount = 1 )
    {
        var author = ctx.User;
        var userData = await DatabaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateUserDataAsync((long)author.Id);


        var embedToBuild = new DiscordEmbedBuilder()
            .WithUser(author)
            .WithTitle("Hmm")
            .WithColor(userData.Color)
            .WithDescription($"You cannot hunt because you have not yet become an adventurer with {Tier.Unranked}");
        if (enemyCount < 1)
        {
            embedToBuild.WithDescription("There must be at least one enemy");
            await ctx.CreateResponseAsync(embedToBuild);
            return;
        }

        if (userData.IsOccupied)
        {
            embedToBuild
                .WithDescription("You are occupied!");
            await ctx.CreateResponseAsync(embedToBuild);
            return;
        }

        if (userData.Tier == Tier.Unranked)
        {
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
        }
        
        var characterType = DefaultObjects.AllAssemblyTypes.FirstOrDefault(
            i =>  i.Name.ToLower() == characterName.ToLower().Replace(" ", "") && i.IsSubclassOf(typeof(Character)) && !i.IsRelatedToType(typeof(Player)));
        if (characterType is null)
        {
            embedToBuild =
                embedToBuild.WithDescription($"Mob {characterName} does not exist!");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
        }

        await MakeOccupiedAsync(userData);
        CharacterTeam enemyTeam = new CharacterTeam();
        foreach (var i in Enumerable.Range(0,(int) enemyCount))
        {
            enemyTeam.Add((Character)Activator.CreateInstance(characterType)!);
        }
      

        embedToBuild = embedToBuild
            .WithTitle($"Keep your guard up!")
            .WithDescription($"Wild {enemyTeam.First()}(s) have appeared!");
        await ctx.CreateResponseAsync(embedToBuild.Build());
        var message = await ctx.GetOriginalResponseAsync();
        var userTeam = await userData.EquippedPlayerTeam.LoadTeamGearWithPlayerDataAsync(author);
        foreach (var i in enemyTeam)
        {
            i.SetLevel(userTeam.Select(i => i.Level).Average().Round());
        }

        var simulator = new BattleSimulator(userTeam, await enemyTeam.LoadTeamGearWithPlayerDataAsync(author));

 

        var battleResult = await simulator.StartAsync(message);



        var expToGain = battleResult.ExpToGain;
        if (battleResult.Winners != userTeam)
        {
            expToGain /= 2;
        }
        string expGainText = userTeam.IncreaseExp(expToGain);
        
        if (battleResult.Winners == userTeam)
        {
            var rewardText = userData.ReceiveRewards(author.Username, battleResult.BattleRewards);
            embedToBuild
                .WithTitle($"Nice going bud!")
                .WithDescription("You won!\n" + expGainText  +$"\n{rewardText}")
                .WithImageUrl("");

            await message.ModifyAsync(new DiscordMessageBuilder(){Embed = embedToBuild.Build() });
        }
        else
        {
            string additionalString = "";
            if (battleResult.TimedOut is not null)
                additionalString += "timed out\n";
            if (battleResult.Forfeited is not null)
                additionalString += "forfeited";
            
            embedToBuild
                .WithTitle($"Ah, too bad\n"+additionalString)
                .WithDescription($"You lost boii\n"+expGainText);
            await message.ModifyAsync(new DiscordMessageBuilder(){Embed = embedToBuild.Build()});
            
        }

        await DatabaseContext.SaveChangesAsync();

    }
}