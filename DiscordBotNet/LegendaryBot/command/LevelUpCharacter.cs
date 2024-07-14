using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using DiscordInteractionResponseBuilder = DSharpPlus.Entities.DiscordInteractionResponseBuilder;

namespace DiscordBotNet.LegendaryBot.command;

public class LevelUpCharacter : GeneralCommandClass
{
    private static ConcurrentDictionary<string,Image<Rgba32>> _cachedLevelUpCroppedImages = new();




    private static ConcurrentDictionary<string, Image<Rgba32>> _resizedBlessingsLevelUpImageCache = new();

    public static async Task<Image<Rgba32>> GetImageForLevelUpAndAscensionAsync( Character character)
    {
        var image = new Image<Rgba32>(495, 300);
        var url = character.ImageUrl;
        if (!_cachedLevelUpCroppedImages.TryGetValue(url, out var characterImage))
        {
            characterImage = await BasicFunctionality.GetImageFromUrlAsync(url);
            characterImage.Mutate(ctx => ctx
                .Resize(new Size(105, 105)));

            _cachedLevelUpCroppedImages[url] = characterImage;
        }
        var font = SystemFonts.CreateFont(Bot.GlobalFontName, 20);
        var expFont = SystemFonts.CreateFont(Bot.GlobalFontName, 25);
        var requiredExpNextLevel = character.GetRequiredExperienceToNextLevel();
        var statsFont = SystemFonts.CreateFont(Bot.GlobalFontName, 15);
        var statsStringBuilder = new StringBuilder();
        image.Mutate(ctx =>
            {

               
                
                foreach (var i in Enum.GetValues<StatType>())
                {
                    statsStringBuilder.Append($"{BasicFunctionality.Englishify(i.ToString())}: {character.GetStatFromType(i)}\n");
                    
                }

                ctx.DrawImage(characterImage,
                        new Point(15, 15), new GraphicsOptions())
                    .BackgroundColor(DiscordColor.Gray.ToImageSharpColor())
                    .Fill(SixLabors.ImageSharp.Color.Blue,
                        new RectangleF(new PointF(135, 80),
                            new SizeF(300 * ((character.Experience * 1.0f) / requiredExpNextLevel), 35)))
                    .Draw(SixLabors.ImageSharp.Color.Black, 4f, new RectangleF(new PointF(135, 80),
                        new SizeF(300, 35)))
                    .DrawText($"Name: {character.Name}\nLevel: {character.Level}/{character.MaxLevel}",
                        font,
                        SixLabors.ImageSharp.Color.Black, new PointF(135, 30))
                    .DrawText($"{character.Experience}/{requiredExpNextLevel}", expFont, SixLabors.ImageSharp.Color.Black,
                        new PointF(160, 85))
                    .DrawText(statsStringBuilder.ToString(), statsFont, SixLabors.ImageSharp.Color.Black,
                        new PointF(10, 130));
       
            }
        );
        if (character.Blessing is not null)
        {
            if (!_resizedBlessingsLevelUpImageCache.TryGetValue(character.Blessing.ImageUrl,
                    out var gottenBlessingImage))
            {
                gottenBlessingImage = await BasicFunctionality.GetImageFromUrlAsync(character.Blessing.ImageUrl);
                gottenBlessingImage.Mutate(ctx =>
                {
                    ctx.Resize(new Size(100, 100));
                });
                _resizedBlessingsLevelUpImageCache[character.Blessing.ImageUrl] = gottenBlessingImage;
            }
            image.Mutate(ctx =>
            {
                ctx.DrawImage(gottenBlessingImage, new Point(image.Width - 100, image.Height - 100),
                    new GraphicsOptions());
            });
        }

        if (character.UserData is not null)
        {
            
            var expUpgradeMat = character.UserData.Inventory.OfType<CharacterExpMaterial>()
                .OrderBy(i => i.ExpToIncrease);
            var ascensionMats = character.UserData.Inventory.OfType<AscensionMaterial>();
            var sum = expUpgradeMat.Cast<Item>().Union(ascensionMats);
            var xOffset = 200;
            foreach (var i in sum)
            {
                var imageToDraw = await i.GetImageAsync();
                imageToDraw.Mutate(j => j.Resize(new Size(60,60)));
                image.Mutate(j =>
                    {
                        j.DrawImage(imageToDraw, new Point(xOffset, 130),new GraphicsOptions());
                    });

                xOffset += 70;
            }

            string? text = null;
            if (character.Level < character.MaxLevel)
            {
                if (character.UserData.Inventory.OfType<CharacterExpMaterial>().Select(i => i.Stacks).Sum() <= 0)
                {
                    text = "You do not have any EXP material";
                }
            }
            else
            {
                text = GetCannotAscendReasonText(character);
            }
         
            if (text is not null)
            {
                var options = new RichTextOptions(statsFont)
                {
                    WrappingLength = 350,
                    Origin = new Vector2(10,260)
                };
                image.Mutate(i => i.DrawText(options,text,SixLabors.ImageSharp.Color.Black));
            }
        }

        return image;
    }
    static bool CanAscend(Character character)
    {
        return GetCannotAscendReasonText(character) is  null && character.Level < character.MaxLevel;
    }
    static bool CanLevelUp(Character character)
    {
        var gottenUserData = character.UserData!;
        var itemsInInventory = gottenUserData.Inventory.OfType<CharacterExpMaterial>();

                
        return character.Level < character.MaxLevel &&
               itemsInInventory.Any(i => i.Stacks > 0);

    }

    static bool ConditionsAreMet(Character character)
    {
        return CanAscend(character) || CanLevelUp(character);
    }
    public static string? GetCannotAscendReasonText(Character character)
    {
        string? failureText = null;
        var gottenUserData = character.UserData;

        if (character.Ascension >= Character.MaxAscensionLevel)
        {
            failureText = $"Max ascension reached. Cannot ascend any further";
        }
        else if((gottenUserData?.Inventory.GetItemStacks<AscensionMaterial>()).GetValueOrDefault(0) < character.RequiredAscensionMaterialsToAscend)
        {
            failureText = $"You do not have enough character ascension materials to ascend (you need {character.RequiredAscensionMaterialsToAscend})";
        }
        else if(!Character.TierCanAscendCharacterInto((gottenUserData?.Tier).GetValueOrDefault(Tier.Unranked),
                    character.Ascension + 1))
        {
            failureText = $"You need to be at least " +
                          $"{Character.GetMinimumTierToAscendCharacterTo(character.Ascension + 1).ToString().Englishify()}" +
                          $" to ascend this character";
        }

        return failureText;
    }
    
    [Command("level_up"),
     AdditionalCommand("/level_up player",BotCommandType.Battle)]
    public async Task LevelUp(CommandContext ctx,
        [Parameter("character_name")] string characterName)
    {
        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");

        Expression<Func<UserData,IEnumerable<Entity>>> includeLambda = (UserData i) => i.Inventory.Where( j => (j is Character
                                 && EF.Property<string>(j, "Discriminator").ToLower() == simplifiedCharacterName) || j is CharacterExpMaterial
            || j is AscensionMaterial);

      

        
        var gottenUserData =await  DatabaseContext.UserData

            .Include(includeLambda)
            .ThenInclude((Entity i) => (i as Character)!.Gears)
            .ThenInclude(i => i.Stats)
            .Include(includeLambda)
            .ThenInclude((Entity i) => (i as Character)!.Blessing)
            .FindOrCreateUserDataAsync(ctx.User.Id);
        var character = gottenUserData.Inventory.OfType<Character>().FirstOrDefault();
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithImageUrl("attachment://levelupimage.png")
            .WithColor(gottenUserData.Color)
            .WithDescription($"Unable to find character with the name \"{simplifiedCharacterName}\"");
        
        if (character is null)
        {
            
            await ctx.RespondAsync(embedBuilder.Build());
            
        }
        else
        {
            
            character.LoadGear();
            embedBuilder.WithTitle($"{character.Name}'s level up process");
            using var levelUpImage = await GetImageForLevelUpAndAscensionAsync(character);
            await using var stream = new MemoryStream();
            await levelUpImage.SaveAsPngAsync(stream);
            stream.Position = 0;
            var levelUp = new DiscordButtonComponent(DiscordButtonStyle.Primary, "level_up", "Level Up");
            var levelToMax = new DiscordButtonComponent(DiscordButtonStyle.Primary, "level_to_max", "Level To Max");
            var ascend = new DiscordButtonComponent(DiscordButtonStyle.Primary, "ascend", "Ascend");
            var stop = new DiscordButtonComponent(DiscordButtonStyle.Danger, "stop", "Stop");
            DiscordComponent[] components = [levelUp, levelToMax, ascend,stop];

            
            var messageBuilder =new DiscordInteractionResponseBuilder()
                .AddEmbed(embedBuilder.Build())
                .AddFile("levelupimage.png", stream);


            

            void SetupComponents()
            {
                if (!CanAscend(character))
                    ascend.Disable();
                else
                {
                    ascend.Enable();
                }
                if (!CanLevelUp(character))
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
     

            var shouldContinue = ConditionsAreMet(character);
            if (shouldContinue)
            {
                SetupComponents();
                messageBuilder.AddComponents(components);
            }
            
            await ctx.RespondAsync(messageBuilder);
            
            if (!shouldContinue)
            {
                return;
            }

            await MakeOccupiedAsync(gottenUserData);
            
            var message = (await ctx.GetResponseAsync())!;
            DiscordInteraction lastInteraction = null!;

            async Task StopAsync()
            {
                character.LoadGear();
                using var localLevelUpImage = await GetImageForLevelUpAndAscensionAsync(character);
                await using var localStream = new MemoryStream();
                await localLevelUpImage.SaveAsPngAsync(localStream);
                localStream.Position = 0;
                if (lastInteraction.ResponseState != DiscordInteractionResponseState.Replied)
                {
                    await lastInteraction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder()
                            .WithTitle("Hmm")
                            .AddFile("levelupimage.png", localStream)
                            .AddEmbed(embedBuilder.Build()));
                }
                else
                {
                    await message.ModifyAsync(new DiscordMessageBuilder()
                        .AddEmbed(embedBuilder.Build())
                        .AddFile("levelupimage.png", localStream));
                }
            }

            while (true)
            {
                
                if(!ConditionsAreMet(character))
                {
                    
                    await StopAsync();

                    break;
                }
                var result = await message!.WaitForButtonAsync(ctx.User,new TimeSpan(0,5,0));



                await DatabaseContext.Entry(gottenUserData).ReloadAsync();
                foreach (var i in DatabaseContext
                             .Entity
                             .Where(i => i is CharacterExpMaterial || i is AscensionMaterial)
                             .ToArray())
                {
                    await DatabaseContext.Entry(i).ReloadAsync();
                }
                if (result.TimedOut)
                {
             
                    await StopAsync();
                    
                    return;
                }
                lastInteraction = result.Result.Interaction;
                if (!ConditionsAreMet(character))
                {
                    await StopAsync();
                    return;
                }
                var decision = result.Result.Interaction.Data.CustomId;



                var expUpgradeMaterials = gottenUserData.Inventory.OfType<CharacterExpMaterial>()
                    .Where(i => i.Stacks > 0)
                    .OrderBy(i => i.Rarity).ToList();
                switch (decision)
                {
                    case "stop":
                        await StopAsync();
                        return;
                    case "ascend":
        
                        if (CanAscend(character))
                        {
                            var requiredMats = character.RequiredAscensionMaterialsToAscend;
                            gottenUserData.Inventory.RemoveItemStacks<AscensionMaterial>(requiredMats);
                            character.Ascension++;
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
                            gottenUserData.Inventory.RemoveItemStacks(firstExpUpgrade.GetType(),1);
                            
                          
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
                            gottenUserData.Inventory.RemoveItemStacks(firstExpUpgrade.GetType(),1);
                        }
            
                        break;
                }

                
                await DatabaseContext.SaveChangesAsync();
                character.LoadGear();
                using var localLevelUpImage = await GetImageForLevelUpAndAscensionAsync(character);
                await using var localStream = new MemoryStream();
                await localLevelUpImage.SaveAsPngAsync(localStream);
                localStream.Position = 0;
                SetupComponents();
                messageBuilder = new DiscordInteractionResponseBuilder()
                    .WithTitle("Hmm")
                    .AddFile("levelupimage.png", localStream)
                    .AddComponents(components)
                    .AddEmbed(embedBuilder.Build());
                
                await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, messageBuilder);



            }
        }

    }
}