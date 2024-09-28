using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using PublicInfo;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using DiscordInteractionResponseBuilder = DSharpPlus.Entities.DiscordInteractionResponseBuilder;

namespace DiscordBot.Commands;

public partial class CharacterCommand
{
    private static readonly ConcurrentDictionary<string, Image<Rgba32>> _cachedLevelUpCroppedImages = new();
    public static  async Task<Image<Rgba32>> GetImageAsync(Item item)
    {
       
          var  stacks = item. Stacks;
        var url =item. ImageUrl;


        var image = await ImageFunctions.GetImageFromUrlAsync(url);

        var theMin = Math.Min(image.Width, image.Height);
        image.Mutate(i => i.Resize(new Size(theMin, theMin)));
        image.Mutate(ctx =>
        {
            var size = 9 * (theMin / 25.0f);
            var x = 1 * (theMin / 25.0f);
            float xOffset = 0;
            var stacksString = stacks.ToString();
            if (stacksString.Length > 1)
            {
                x = 0;
                xOffset = (stacksString.Length - 1) * 3 * (theMin / 25.0f);
            }

            ctx.Fill(SixLabors.ImageSharp.Color.Black, new RectangleF(0, 0, size + xOffset, size));
            var font = SystemFonts.CreateFont(Information.GlobalFontName, size, FontStyle.Bold);

            ctx.DrawText(stacksString, font,SixLabors.ImageSharp.Color.White, new PointF(x, 0));
        });

        return image;
    }

    private static readonly ConcurrentDictionary<string, Image<Rgba32>> _resizedBlessingsLevelUpImageCache = new();

    public static void UpdateEmbed(DiscordEmbedBuilder builder, Character character)
    {
        var description = $"";
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<CharacterExpMaterial>())
            description += $"{i.Name}: {character.UserData!.Items.GetItemStacks(i.GetType()):N0}\n";

        if (character.Blessing is not null) description += $"Blessing: {character.Blessing.Name}";


        builder.WithDescription(description);
    }

    public static async Task<Image<Rgba32>> GetImageForLevelUpAndAscensionAsync(Character character)
    {
        var image = new Image<Rgba32>(495, 300);
        var url = character.ImageUrl;
        if (!_cachedLevelUpCroppedImages.TryGetValue(url, out var characterImage))
        {
            characterImage = await ImageFunctions.GetImageFromUrlAsync(url);
            characterImage.Mutate(ctx => ctx
                .Resize(new Size(105, 105)));

            _cachedLevelUpCroppedImages[url] = characterImage;
        }

        var font = SystemFonts.CreateFont(Information.GlobalFontName, 20);
        var expFont = SystemFonts.CreateFont(Information.GlobalFontName, 25);
        var requiredExpNextLevel = character.GetRequiredExperienceToNextLevel();
        var statsFont = SystemFonts.CreateFont(Information.GlobalFontName, 15);
        var statsStringBuilder = new StringBuilder();
        image.Mutate(ctx =>
            {
                foreach (var i in Enum.GetValues<StatType>())
                    statsStringBuilder.Append($"{i.GetShortName()}: {character.GetStatFromType(i)}\n");

                ctx.DrawImage(characterImage,
                        new Point(15, 15), new GraphicsOptions())
                    .BackgroundColor(SixLabors.ImageSharp.Color.Gray)
                    .Fill(SixLabors.ImageSharp.Color.Blue,
                        new RectangleF(new PointF(135, 80),
                            new SizeF(300 * (character.Experience * 1.0f / requiredExpNextLevel), 35)))
                    .Draw(SixLabors.ImageSharp.Color.Black, 4f, new RectangleF(new PointF(135, 80),
                        new SizeF(300, 35)))
                    .DrawText($"Name: {character.Name}\nLevel: {character.Level}/{character.MaxLevel}",
                        font,
                        SixLabors.ImageSharp.Color.Black, new PointF(135, 30))
                    .DrawText($"{character.Experience}/{requiredExpNextLevel}", expFont,
                        SixLabors.ImageSharp.Color.Black,
                        new PointF(160, 85))
                    .DrawText($"\nDivine Echoes: {character.DivineEcho}",font,
                        SixLabors.ImageSharp.Color.Black,new Point(280,30))
                    .DrawText(statsStringBuilder.ToString(), statsFont, SixLabors.ImageSharp.Color.Black,
                        new PointF(10, 130));
            }
        );
        if (character.Blessing is not null)
        {
            if (!_resizedBlessingsLevelUpImageCache.TryGetValue(character.Blessing.ImageUrl,
                    out var gottenBlessingImage))
            {
                gottenBlessingImage = await ImageFunctions.GetImageFromUrlAsync(character.Blessing.ImageUrl);
                gottenBlessingImage.Mutate(ctx => { ctx.Resize(new Size(100, 100)); });
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
                using var imageToDraw = await GetImageAsync(i);
                imageToDraw.Mutate(j => j.Resize(new Size(60, 60)));
                image
                    .Mutate(j => { j.DrawImage(imageToDraw, new Point(xOffset, 130), new GraphicsOptions()); });

                xOffset += 70;
            }

            string? text = null;
            if (character.Level < character.MaxLevel)
            {
                if (character.UserData.Items.OfType<CharacterExpMaterial>().Select(i => i.Stacks).Sum() <= 0)
                    text = "You do not have any EXP material";
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
                    Origin = new Vector2(10, 265)
                };
                image.Mutate(i => i.DrawText(options, text, SixLabors.ImageSharp.Color.Black));
            }
        }

        return image;
    }

    private static bool CanLevelUp(Character character)
    {
        var gottenUserData = character.UserData!;
        var itemsInInventory = gottenUserData.Items.OfType<CharacterExpMaterial>();


        return character.Level < character.MaxLevel &&
               itemsInInventory.Any(i => i.Stacks > 0);
    }

    private static bool ConditionsAreMet(Character character)
    {
        return CanLevelUp(character);
    }


    [Command("level-up")]
    [Description("used to level up a character. Also used to display stats and blessing")]
    [BotCommandCategory(BotCommandCategory.Character)]
    public async ValueTask ExecuteLevelUp(CommandContext ctx,
        [Parameter("character-name")] string characterName)
    {
        var typeIdToLookFor = Character.LookFor(characterName);
        Expression<Func<UserData, IEnumerable<Character>>> includeLambda = i =>
            i.Characters.Where(j =>
                j.TypeId == typeIdToLookFor);

        var gottenUserData = await DatabaseContext.Set<UserData>()
            .Include(includeLambda)
            .ThenInclude(i => i.Gears)
            .ThenInclude(i => i.Stats)
            .Include(includeLambda)
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Items.Where(j => j is CharacterExpMaterial))
            .FirstOrDefaultAsync(i => i.DiscordId == ctx.User.Id);

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
            .WithDescription($"Unable to find character with the name \"{characterName}\"");

        if (gottenUserData.IsOccupied)
        {
            embedBuilder.WithDescription("You are occupied")
                .WithImageUrl((string)null!);
            await ctx.RespondAsync(embedBuilder);
            return;
        }

        gottenUserData.Inventory.MergeItemStacks();
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
                    stop.Enable();
                else
                    stop.Disable();
            }


            UpdateEmbed(embedBuilder, character);
            var messageBuilder = new DiscordInteractionResponseBuilder()
                .AddEmbed(embedBuilder.Build())
                .AddFile("levelupimage.png", stream);

            SetupComponents();
            messageBuilder.AddComponents(components);


            await ctx.RespondAsync(messageBuilder);

            if (!ConditionsAreMet(character)) return;

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
                foreach (var i in components) i.Disable();
                if (lastInteraction.ResponseState != DiscordInteractionResponseState.Replied)
                    await lastInteraction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder()
                            .WithTitle("Hmm")
                            .AddComponents(components)
                            .AddFile("levelupimage.png", localStream)
                            .AddEmbed(embedBuilder.Build()));
                else
                    await message.ModifyAsync(new DiscordMessageBuilder()
                        .AddComponents(components)
                        .AddEmbed(embedBuilder.Build())
                        .AddFile("levelupimage.png", localStream));
            }

            while (true)
            {
                if (!ConditionsAreMet(character))
                {
                    await StopAsync();

                    break;
                }

                var result = await message!.WaitForButtonAsync(ctx.User, new TimeSpan(0, 5, 0));

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
                            if (character.Level > previousLevel || character.Level >= character.MaxLevel)
                                break;
                            if (!expUpgradeMaterials.Any())
                                break;
                            var firstExpUpgrade = expUpgradeMaterials[0];

                            if (firstExpUpgrade.Stacks <= 0)
                            {
                                expUpgradeMaterials.Remove(firstExpUpgrade);
                                continue;
                            }

                            character.IncreaseExp(firstExpUpgrade.ExpToIncrease);
                            gottenUserData.Items.RemoveItemStacks(firstExpUpgrade.GetType(), 1);
                        }


                        break;
                    case "level_to_max":

                        while (true)
                        {
                            if (character.Level >= character.MaxLevel)
                                break;
                            if (!expUpgradeMaterials.Any())
                                break;
                            var firstExpUpgrade = expUpgradeMaterials[0];

                            if (firstExpUpgrade.Stacks <= 0)
                            {
                                expUpgradeMaterials.Remove(firstExpUpgrade);
                                continue;
                            }

                            character.IncreaseExp(firstExpUpgrade.ExpToIncrease);
                            gottenUserData.Items.RemoveItemStacks(firstExpUpgrade.GetType(), 1);
                        }

                        break;
                }


                await DatabaseContext.SaveChangesAsync();
                character.LoadStats();
                UpdateEmbed(embedBuilder, character);

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

                await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                    messageBuilder);
            }
        }
    }
}