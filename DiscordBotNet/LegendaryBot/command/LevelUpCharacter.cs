using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text;
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
        
        image.Mutate(ctx =>
            {
                var font = SystemFonts.CreateFont(Bot.GlobalFontName, 20);
                var expFont = SystemFonts.CreateFont(Bot.GlobalFontName, 25);
                var requiredExpNextLevel = character.GetRequiredExperienceToNextLevel();
                var statsFont = SystemFonts.CreateFont(Bot.GlobalFontName, 15);
                var statsStringBuilder = new StringBuilder();
               
                
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
                .MergeItems()
                .OrderBy(i => i.ExpToIncrease);
            var ascensionMats = character.UserData.Inventory.OfType<AscensionMaterial>().MergeItems();
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
        }

        return image;
    }
    private static readonly string totalAscensionStuff = "Cannot upgrade your character because you cannot ascend," +
                                 "nor level up your character. ensure you have EXP materials and you the character hasnt reached max level," +
                                 "or you are max level, and have enough ascension materials, and your tier is also sufficient enough to ascend." +
                                 "(silver to ascend to max level 20, gold to ascend to max level 30, platinum to ascend to max level 40, " +
                                 $"diamond ot ascend to max level 50, {Tier.Divine.ToString().ToLower()} to ascend to max level 60";


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
            .ThenInclude((Entity i) => (i as Character).Gears)
            .ThenInclude(i => i.Stats)
            .Include(includeLambda)
            .ThenInclude((Entity i) => (i as Character).Blessing)
            .FindOrCreateUserDataAsync((long)ctx.User.Id);
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

            bool CanAscend()
            {
                var itemsInInventory = gottenUserData.Inventory.OfType<Item>().ToArray();

                var ascensionMat = itemsInInventory.OfType<AscensionMaterial>()
                    .MergeItems().FirstOrDefault();
                
                return
                    character.Level < character.MaxLevel  && character.Ascension < Character.MaxAscensionLevel &&  gottenUserData.CanAscendCharactersTo(character.Ascension + 1)
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
            
            await ctx.RespondAsync(messageBuilder);
            
            if (!shouldContinue)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                    .WithContent(totalAscensionStuff)
                    .AsEphemeral());
            
                return;
            }

          
            await MakeOccupiedAsync(gottenUserData);
            var message = await ctx.GetResponseAsync();
            DiscordInteraction lastInteraction = null!;
            while (true)
            {
                
                if(!ConditionsAreMet())
                {
                    
                    await StopAsync();
                    await lastInteraction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AsEphemeral()
                        .WithContent(totalAscensionStuff));
                    break;
                }
                var result = await message.WaitForButtonAsync(ctx.User,new TimeSpan(0,5,0));
         
                if (result.TimedOut)
                {
             
                    await StopAsync();
                    
                    return;
                }
                lastInteraction = result.Result.Interaction;
                if (!ConditionsAreMet())
                {
                    await StopAsync();
                    await lastInteraction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AsEphemeral()
                        .WithContent(totalAscensionStuff));
                    return;
                }
                var decision = result.Result.Interaction.Data.CustomId;

                async Task StopAsync()
                {
                    character.LoadGear();
                    using var localLevelUpImage = await GetImageForLevelUpAndAscensionAsync(character);
                    await using var localStream = new MemoryStream();
                    await localLevelUpImage.SaveAsPngAsync(localStream);
                    localStream.Position = 0;
                    if (lastInteraction.ResponseState == DiscordInteractionResponseState.Replied)
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
                var ascensionMaterial = gottenUserData.Inventory.OfType<AscensionMaterial>().MergeItems().FirstOrDefault(new AscensionMaterial(){Stacks = 0});
                var expUpgradeMaterials = gottenUserData.Inventory.OfType<ExpIncreaseMaterial>().MergeItems()
                    .OrderBy(i => i.ExpToIncrease)
                    .ToList();
                string failureText = null;
                switch (decision)
                {
                    case "stop":
                        await StopAsync();
                        return;
                    case "ascend":
                        if (CanAscend() && ascensionMaterial.Stacks >= character.RequiredAscensionMaterialsToAscend)
                        {
                            var requiredMats = character.RequiredAscensionMaterialsToAscend;
                            ascensionMaterial.Stacks -= requiredMats;
                        }
                        else
                        {
                            failureText =
                                $"Cannot ascend character. character must reach max level (in this case, it's {character.Level})  and you need ascension materials";
                        }
                        break;
                    case "level_up":
                        var previousLevel = character.Level;
                        var expIncreasedAtLeastOnce = false;
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
                            expIncreasedAtLeastOnce = true;
                        }

                        if (!expIncreasedAtLeastOnce)
                        {
                            failureText =
                                "Cannot increase exp of character. ensure character hasnt reached max level, and you have exp upgrade materials";
                        }
                        
                        break;
                    case "level_to_max":
                        expIncreasedAtLeastOnce = false;
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

                            expIncreasedAtLeastOnce = true;
                            character.IncreaseExp(firstExpUpgrade.ExpToIncrease);
                            firstExpUpgrade.Stacks--;
                        }
                        if (!expIncreasedAtLeastOnce)
                        {
                            failureText =
                                "Cannot increase exp of character. ensure character hasnt reached max level, and you have exp upgrade materials";
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