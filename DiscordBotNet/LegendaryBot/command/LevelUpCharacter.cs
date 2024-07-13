using System.Linq.Expressions;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using DiscordInteractionResponseBuilder = DSharpPlus.Entities.DiscordInteractionResponseBuilder;

namespace DiscordBotNet.LegendaryBot.command;

public class LevelUpCharacter : GeneralCommandClass
{
    [SlashCommand("level_up", "leveling up character"),
     AdditionalSlashCommand("/level_up player",BotCommandType.Battle)]
    public async Task LevelUp(InteractionContext ctx,
        [Option("character_name", "the name of the character")] string characterName)
    {
        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");

        Expression<Func<UserData,IEnumerable<Entity>>> includeLambda = (UserData i) => i.Inventory.Where( j => (j is Character
                                 && EF.Property<string>(j, "Discriminator").ToLower() == simplifiedCharacterName) || j is CharacterExpMaterial
            || j is AscensionMaterial);



        var gottenUserData =await  DatabaseContext.UserData

            .Include(includeLambda)
            .ThenInclude((Entity i) => (i as Character).Gears)
            .ThenInclude(i => i.Stats)
            .Include(includeLambda)
            .ThenInclude((Entity i) => (i as Character).Blessing)
            .FindOrCreateUserDataAsync((long)ctx.User.Id);
        Character character = gottenUserData.Inventory.OfType<Character>().FirstOrDefault();
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithImageUrl("attachment://levelupimage.png")
            .WithColor(gottenUserData.Color)
            .WithDescription($"Unable to find character with the name \"{simplifiedCharacterName}\"");
        
        if (character is null)
        {
            await ctx.CreateResponseAsync(embedBuilder.Build());
            
        }
        else
        {

            character.LoadGear();
            embedBuilder.WithTitle($"{character.Name}'s level up process");
            using var levelUpImage = await character.GetImageForLevelUpAndAscensionAsync();
            await using var stream = new MemoryStream();
            await levelUpImage.SaveAsPngAsync(stream);
            stream.Position = 0;
            var levelUp = new DiscordButtonComponent(ButtonStyle.Primary, "level_up", "Level Up");
            var levelToMax = new DiscordButtonComponent(ButtonStyle.Primary, "level_to_max", "Level To Max");
            var ascend = new DiscordButtonComponent(ButtonStyle.Primary, "ascend", "Ascend");
            var stop = new DiscordButtonComponent(ButtonStyle.Danger, "stop", "Stop");
            DiscordComponent[] components = [levelUp, levelToMax, ascend,stop];

            
            var messageBuilder =new DiscordInteractionResponseBuilder()
                .AddEmbed(embedBuilder.Build())
                .AddFile("levelupimage.png", stream);

            bool CanAscend()
            {
                var itemsInInventory = gottenUserData.Inventory.OfType<Item>().ToArray();

                var ascensionMat = itemsInInventory.OfType<AscensionMaterial>()
                    .MergeItems().FirstOrDefault();
                
                return
                    character.CanAscend && gottenUserData.CanAscendCharactersTo(character.Ascension + 1)
                                        && ascensionMat is not null &&  ascensionMat.Stacks >= character.RequiredAscensionMaterialsToAscend;
            }

            bool CanLevelUp()
            {
                var itemsInInventory = gottenUserData.Inventory.OfType<Item>().ToArray();

                
                return character.Level < character.MaxLevel &&
                                 itemsInInventory.OfType<CharacterExpMaterial>().Any(i => i.Stacks > 0);

            }

            void SetupComponents()
            {
                if (!CanAscend())
                    ascend.Disable();
                else
                {
                    ascend.Enable();
                }
                if (!CanLevelUp())
                {
                    levelUp.Disable();
                    levelToMax.Disable();
                }
                else
                {
                    levelUp.Enable();
                    levelToMax.Enable();
                }
            }
            bool ConditionsAreMet()
            {

                return CanLevelUp() || CanAscend();
            }

            var shouldContinue = ConditionsAreMet();
            if (shouldContinue)
            {
                SetupComponents();
                messageBuilder.AddComponents(components);
            }
            await ctx.CreateResponseAsync(messageBuilder);
            if(!shouldContinue)
                return;
            await MakeOccupiedAsync(gottenUserData);
            var message = await ctx.GetOriginalResponseAsync();
            DiscordInteraction lastInteraction = null!;
            while (true)
            {
                if (!ConditionsAreMet())
                {
                    lastInteraction = null;
                    await StopAsync();
                    return;
                }
                var result = await message.WaitForButtonAsync(ctx.User,new TimeSpan(0,5,0));
         
                if (result.TimedOut)
                {
                    lastInteraction = null;
                    await StopAsync();
                    return;
                }
                lastInteraction = result.Result.Interaction;
                if (!ConditionsAreMet())
                {
                    await StopAsync();
                    return;
                }
                var decision = result.Result.Interaction.Data.CustomId;

                async Task StopAsync()
                {
                    using var localLevelUpImage = await character.GetImageForLevelUpAndAscensionAsync();
                    await using var localStream = new MemoryStream();
                    await localLevelUpImage.SaveAsPngAsync(localStream);
                    localStream.Position = 0;
                    messageBuilder = new DiscordInteractionResponseBuilder()
                        .WithTitle("Hmm")
                        .AddFile("levelupimage.png", localStream)
                        .AddEmbed(embedBuilder.Build());
                
                    
                    
                    if (lastInteraction is not null) 
                        await lastInteraction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                        messageBuilder);
                    else
                    {
                        await message.ModifyAsync(   new DiscordMessageBuilder(messageBuilder));
                    }
                }
                var ascensionMaterial = gottenUserData.Inventory.OfType<AscensionMaterial>().MergeItems().FirstOrDefault(new AscensionMaterial(){Stacks = 0});
                var expUpgradeMaterials = gottenUserData.Inventory.OfType<ExpIncreaseMaterial>().MergeItems()
                    .OrderBy(i => i.ExpToIncrease)
                    .ToList();
                switch (decision)
                {
                    case "stop":
                        await StopAsync();
                        return;
                    case "ascend":
                        if (character.CanAscend && ascensionMaterial.Stacks >= character.RequiredAscensionMaterialsToAscend)
                        {
                            var requiredMats = character.RequiredAscensionMaterialsToAscend;
                            var ascended =character.Ascend();
                            if (ascended)
                                ascensionMaterial.Stacks -= requiredMats;
                        }
                        break;
                    case "level_up":
                        var previousLevel = character.Level;
                        while (true)
                        {
                            if(character.Level > previousLevel || character.Level >= character.MaxLevel)
                                break;
                            if(!expUpgradeMaterials.Any())
                                break;
                            var firstExpUpgrade = expUpgradeMaterials[0];
                            
                            if (firstExpUpgrade.Stacks <= 0)
                            {
                                expUpgradeMaterials.Remove(firstExpUpgrade);
                                continue;
                                
                            }

                            character.IncreaseExp(firstExpUpgrade.ExpToIncrease);
                            firstExpUpgrade.Stacks--;
                            
                        }
                        break;
                    case "level_to_max":
                     
                        while (true)
                        {
                            if(character.Level >= character.MaxLevel)
                                break;
                            if(!expUpgradeMaterials.Any())
                                break;
                            var firstExpUpgrade = expUpgradeMaterials[0];
                            
                            if (firstExpUpgrade.Stacks <= 0)
                            {
                                expUpgradeMaterials.Remove(firstExpUpgrade);
                                continue;
                            }
                            character.IncreaseExp(firstExpUpgrade.ExpToIncrease);
                            firstExpUpgrade.Stacks--;
                        }
                        break;
                }

                
                await DatabaseContext.SaveChangesAsync();
                using var localLevelUpImage = await character.GetImageForLevelUpAndAscensionAsync();
                await using var localStream = new MemoryStream();
                await localLevelUpImage.SaveAsPngAsync(localStream);
                localStream.Position = 0;
                SetupComponents();
                messageBuilder = new DiscordInteractionResponseBuilder()
                    .WithTitle("Hmm")
                    .AddFile("levelupimage.png", localStream)
                    .AddComponents(components)
                    .AddEmbed(embedBuilder.Build());
                
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, messageBuilder);


            }
        }

    }
}