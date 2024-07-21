using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using DiscordInteractionResponseBuilder = DSharpPlus.Entities.DiscordInteractionResponseBuilder;

namespace DiscordBotNet.LegendaryBot.Commands;


public class LevelUpCharacter : GeneralCommandClass
{
    private static ConcurrentDictionary<string,Image<Rgba32>> _cachedLevelUpCroppedImages = new();

    
    private static ConcurrentDictionary<string, Image<Rgba32>> _resizedBlessingsLevelUpImageCache = new();

    public static void UpdateEmbed(DiscordEmbedBuilder builder, Character character)
    {
        var description = $"Character number: {character.Number}\n";
        foreach (var i in TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<CharacterExpMaterial>())
        {
            description += $"{i.Name}: {character.UserData.Items.GetItemStacks(i.GetType())}\n";
        }

        if (character.Blessing is not null)
        {
            description += $"Blessing: {character.Blessing.Name}";
        }


        builder.WithDescription(description);
    }
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
                    .DrawText($"{character.Experience}/{requiredExpNextLevel}", expFont,
                        SixLabors.ImageSharp.Color.Black,
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
            
            var expUpgradeMat = character.UserData.Items.OfType<CharacterExpMaterial>()
                .OrderBy(i => i.ExpToIncrease);

            var xOffset = 200;
            foreach (var i in expUpgradeMat)
            {
                using var imageToDraw = await i.GetImageAsync();
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
                if (character.UserData.Items.OfType<CharacterExpMaterial>().Select(i => i.Stacks).Sum() <= 0)
                {
                    text = "You do not have any EXP material";
                }
            }
            else
            {
                text = "Max level reached!";
            }
            
         
            if (text is not null)
            {
                var options = new RichTextOptions(statsFont)
                {
                    WrappingLength = 350,
                    Origin = new Vector2(10,265)
                };
                image.Mutate(i => i.DrawText(options,text,SixLabors.ImageSharp.Color.Black));
            }
        }

        return image;
    }

    static bool CanLevelUp(Character character)
    {
        var gottenUserData = character.UserData!;
        var itemsInInventory = gottenUserData.Items.OfType<CharacterExpMaterial>();

                
        return character.Level < character.MaxLevel &&
               itemsInInventory.Any(i => i.Stacks > 0);

    }

    static bool ConditionsAreMet(Character character)
    {
        return CanLevelUp(character);
    }



    
    [Command("level-up"), Description("used to level up a character. Also used to display stats and blessing"),
     AdditionalCommand("/level-up 1",BotCommandType.Battle)]
    public async ValueTask ExecuteLevelUp(CommandContext ctx,
        [Parameter("character-number")] int characterNumber)
    {


        Expression<Func<UserData, IEnumerable<Character>>> includeLambda = (UserData i) =>
            i.Characters.Where(j =>
                j.Number == characterNumber);
        
        var gottenUserData =await  DatabaseContext.UserData
            .Include(includeLambda)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .Include(includeLambda)
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Items.Where(j =>j is CharacterExpMaterial))
            .FirstOrDefaultAsync(i => i.Id == ctx.User.Id);
   
        if (gottenUserData is null || gottenUserData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithImageUrl("attachment://levelupimage.png")
            .WithColor(gottenUserData.Color)
            .WithDescription($"Unable to find character with the number \"{characterNumber}\"");

        if(gottenUserData.IsOccupied)
        {
            embedBuilder.WithDescription("You are occupied")
                .WithImageUrl((string)null);
            await ctx.RespondAsync(embedBuilder);
            return;
        }
        gottenUserData.Inventory.MergeDuplicates();
        var character = gottenUserData.Characters.FirstOrDefault();

        if (character is null)
        {
            await ctx.RespondAsync(embedBuilder.Build());
        }
        else
        {
            await MakeOccupiedAsync(gottenUserData);
            
            
            character.LoadStats();
            embedBuilder.WithTitle($"{character.Name}'s level up process");
            using var levelUpImage = await GetImageForLevelUpAndAscensionAsync(character);
            await using var stream = new MemoryStream();
            await levelUpImage.SaveAsPngAsync(stream);
            stream.Position = 0;
            var levelUp = new DiscordButtonComponent(DiscordButtonStyle.Primary, "level_up", "Level Up");
            var levelToMax = new DiscordButtonComponent(DiscordButtonStyle.Primary, "level_to_max", "Level To Max");

            var stop = new DiscordButtonComponent(DiscordButtonStyle.Danger, "stop", "Stop");
            DiscordButtonComponent[] components = [levelUp, levelToMax, stop];

            
       

            

            void SetupComponents()
            {
                var anyConditionIsMet = false;
     
         
                if (!CanLevelUp(character))
                {
                   
                    levelUp.Disable();
                    levelToMax.Disable();
                }
                else
                {
                    anyConditionIsMet = true;
                    levelUp.Enable();
                    levelToMax.Enable();
                }

                if (anyConditionIsMet)
                {
                    stop.Enable();
                }
                else
                {
                    stop.Disable();
                }
            }
     

           
            UpdateEmbed(embedBuilder,character);
            var messageBuilder =new DiscordInteractionResponseBuilder()
                .AddEmbed(embedBuilder.Build())
                .AddFile("levelupimage.png", stream);

            SetupComponents();
            messageBuilder.AddComponents(components);
            
       
            await ctx.RespondAsync(messageBuilder);
            
            if (!ConditionsAreMet(character))
            {
                return;
            }

            await MakeOccupiedAsync(gottenUserData);
            
            var message = (await ctx.GetResponseAsync())!;
            DiscordInteraction lastInteraction = null!;

            async Task StopAsync()
            {
                character.LoadStats();
                using var localLevelUpImage = await GetImageForLevelUpAndAscensionAsync(character);
                await using var localStream = new MemoryStream();
                await localLevelUpImage.SaveAsPngAsync(localStream);
                localStream.Position = 0;
                foreach (var i in components)
                {
                    i.Disable();
                }
                if (lastInteraction.ResponseState != DiscordInteractionResponseState.Replied)
                {
                    await lastInteraction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder()
                            .WithTitle("Hmm")
                            .AddComponents(components)
                            .AddFile("levelupimage.png", localStream)
                            .AddEmbed(embedBuilder.Build()));
                }
                else
                {
                    await message.ModifyAsync(new DiscordMessageBuilder()
                        .AddComponents(components)
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



                var expUpgradeMaterials = gottenUserData.Items.OfType<CharacterExpMaterial>()
                    .Where(i => i.Stacks > 0)
                    .OrderBy(i => i.Rarity).ToList();
                switch (decision)
                {
                    case "stop":
                        await StopAsync();
                        return;
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
                            gottenUserData.Items.RemoveItemStacks(firstExpUpgrade.GetType(),1);
                            
                          
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
                            gottenUserData.Items.RemoveItemStacks(firstExpUpgrade.GetType(),1);
                        }
            
                        break;
                }

                
                await DatabaseContext.SaveChangesAsync();
                character.LoadStats();
                UpdateEmbed(embedBuilder,character);
              
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