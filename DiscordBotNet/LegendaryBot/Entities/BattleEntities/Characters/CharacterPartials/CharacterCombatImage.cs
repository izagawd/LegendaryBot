using System.Collections.Concurrent;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using DiscordBotNet.LegendaryBot.Moves;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
        
    /// <summary>
    /// Load build if this character isnt already loaded, or dont load the build if u set stats manually <br/>
    /// eg TotalAttack = 5000;
    /// </summary>
    /// <param name="loadBuild"></param>
    /// <returns></returns>
    public virtual async Task<Image<Rgba32>> GetDetailsImageAsync(bool loadBuild)
    {
        using var characterImageInfo = await GetInfoAsync();
        if(loadBuild)
            LoadGear();
        
        var image = new Image<Rgba32>(850, 900);
        
       
        return image;
    }
    public sealed override   Task<Image<Rgba32>> GetDetailsImageAsync()
    {
        return GetDetailsImageAsync(true);
    }


    /// <summary>
    /// Caches the cropped combat images, since cropping takes time
    /// </summary>
    private static ConcurrentDictionary<string,Image<Rgba32>> _cachedCombatCroppedImages = new();

    private static ConcurrentDictionary<string,Image<Rgba32>> _cachedLevelUpCroppedImages = new();




    private static ConcurrentDictionary<string, Image<Rgba32>> _resizedBlessingsLevelUpImageCache = new();
    public async Task<Image<Rgba32>> GetImageForLevelUpAndAscensionAsync()
    {
        var image = new Image<Rgba32>(495, 300);
        var url = ImageUrl;
        if (!_cachedLevelUpCroppedImages.TryGetValue(url, out Image<Rgba32> characterImage))
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
                var requiredExpNextLevel = GetRequiredExperienceToNextLevel();
                var statsFont = SystemFonts.CreateFont(Bot.GlobalFontName, 15);
                var statsStringBuilder = new StringBuilder();
               
                
                foreach (var i in Enum.GetValues<StatType>())
                {
                    statsStringBuilder.Append($"{BasicFunctionality.Englishify(i.ToString())}: {GetStatFromType(i)}\n");
                    
                }

                ctx.DrawImage(characterImage,
                        new Point(15, 15), new GraphicsOptions())
                    .BackgroundColor(DiscordColor.Gray.ToImageSharpColor())
                    .Fill(SixLabors.ImageSharp.Color.Blue,
                        new RectangleF(new PointF(135, 80),
                            new SizeF(300 * ((Experience * 1.0f) / requiredExpNextLevel), 35)))
                    .Draw(SixLabors.ImageSharp.Color.Black, 4f, new RectangleF(new PointF(135, 80),
                        new SizeF(300, 35)))
                    .DrawText($"Name: {Name}\nLevel: {Level}/{MaxLevel}",
                        font,
                        SixLabors.ImageSharp.Color.Black, new PointF(135, 30))
                    .DrawText($"{Experience}/{requiredExpNextLevel}", expFont, SixLabors.ImageSharp.Color.Black,
                        new PointF(160, 85))
                    .DrawText(statsStringBuilder.ToString(), statsFont, SixLabors.ImageSharp.Color.Black,
                        new PointF(10, 130));
       
            }
        );
        if (Blessing is not null)
        {
            if (!_resizedBlessingsLevelUpImageCache.TryGetValue(Blessing.ImageUrl,
                    out Image<Rgba32> gottenBlessingImage))
            {
                gottenBlessingImage = await BasicFunctionality.GetImageFromUrlAsync(Blessing.ImageUrl);
                gottenBlessingImage.Mutate(ctx =>
                {
                    ctx.Resize(new Size(100, 100));
                });
                _resizedBlessingsLevelUpImageCache[Blessing.ImageUrl] = gottenBlessingImage;
            }
            image.Mutate(ctx =>
            {
                ctx.DrawImage(gottenBlessingImage, new Point(image.Width - 100, image.Height - 100),
                    new GraphicsOptions());
            });
        }

        if (UserData is not null)
        {
            var expUpgradeMat = UserData.Inventory.OfType<CharacterExpMaterial>()
                .MergeItems()
                .OrderBy(i => i.ExpToIncrease);
            var ascensionMats = UserData.Inventory.OfType<AscensionMaterial>().MergeItems();
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
    public async Task<Image<Rgba32>> GetImageForCombatAsync()
    {

        var image = new Image<Rgba32>(190, 150);
        var url = ImageUrl;
        if (!_cachedCombatCroppedImages.TryGetValue(url, out Image<Rgba32> characterImage))
        {
            characterImage = await BasicFunctionality.GetImageFromUrlAsync(url);
            characterImage.Mutate(ctx =>
            {
                ctx.Resize(new Size(50, 50));
            });
            //any image outside of the domain will n=be removed after a certain amount of time using this entry option
            _cachedCombatCroppedImages[url] = characterImage;
        }
 
        IImageProcessingContext ctx = null!;
        image.Mutate(idk => ctx = idk);
       
        ctx
            .DrawImage(characterImage, new Point(0, 0), new GraphicsOptions())
            .Draw(SixLabors.ImageSharp.Color.Black, 1, new Rectangle(new Point(0, 0), new Size(50, 50)))
            .DrawText($"Lvl {Level}", SystemFonts.CreateFont(Bot.GlobalFontName, 10),
        SixLabors.ImageSharp.Color.Black, new PointF(55, 21.5f))
            .Draw(SixLabors.ImageSharp.Color.Black, 1,
        new RectangleF(52.5f, 20, 70, 11.5f))
            .DrawText(Name + $" [{AlphabetIdentifier}] [{Position}]", SystemFonts.CreateFont(Bot.GlobalFontName, 11),
        SixLabors.ImageSharp.Color.Black, new PointF(55, 36.2f))
            .Draw(SixLabors.ImageSharp.Color.Black, 1,
        new RectangleF(52.5f, 35, 115, 12.5f));

        var healthPercentage = HealthPercentage;
        int width = 175;
        var shieldPercentage = ShieldPercentage;
        int filledWidth = (width * healthPercentage / 100.0).Round();
        int filledShieldWidth = (width * shieldPercentage / 100).Round();
        int barHeight = 16; 
        if(healthPercentage < 100)
            ctx.Fill(SixLabors.ImageSharp.Color.Red, new Rectangle(0, 50, width, barHeight));
        ctx.Fill(SixLabors.ImageSharp.Color.Green, new Rectangle(0, 50, filledWidth, barHeight));
        int shieldXPosition =  filledWidth;
        if (shieldXPosition + filledShieldWidth > width)
        {
            shieldXPosition = width - filledShieldWidth;
        }
        if(shieldPercentage > 0)
            ctx.Fill(SixLabors.ImageSharp.Color.White, new RectangleF(shieldXPosition, 50, filledShieldWidth, barHeight));

        // Creates a border for the health bar
        ctx.Draw(SixLabors.ImageSharp.Color.Black, 0.5f, new Rectangle(0, 50, width, barHeight));
        ctx.DrawText($"{Health.Round()}/{MaxHealth.Round()}", SystemFonts.CreateFont(Bot.GlobalFontName, 14),
        SixLabors.ImageSharp.Color.Black, new PointF(2.5f, 51.5f));

        int xOffSet = 0;
        int yOffSet = 50 + barHeight + 5;

        int moveLength = 25; 

        foreach (var i in MoveList)
        {
            //do not change size of the move image here.
            //do it in the method that gets the image
            using var moveImage = await i.GetImageForCombatAsync();
            ctx.DrawImage(moveImage, new Point(xOffSet, yOffSet), new GraphicsOptions());
            xOffSet += moveLength;
            int cooldown = 0;
            if (i is Special special)
            {
                cooldown = special.Cooldown;
            }

            var cooldownString = ""; 
            if (cooldown > 0)
            {
                cooldownString = cooldown.ToString();
            }
            ctx.DrawText(cooldownString, SystemFonts.CreateFont(Bot.GlobalFontName, moveLength),
                SixLabors.ImageSharp.Color.Black, new PointF(xOffSet + 5, yOffSet));
            xOffSet += moveLength;
        }
     

        xOffSet = 0;
        yOffSet += moveLength + 5;

      
        
        foreach (var i in _statusEffects.Take(16))
        {
            
            //do not change size of the status effect image here.
            //do it in the method that gets the image
            using var statusImage = await i.GetImageForCombatAsync();
            var statusLength = statusImage.Size.Width;
            if (xOffSet + statusLength + 2 >= 185)
            {
                xOffSet = 0;
                yOffSet += statusLength + 2;
            }
            ctx.DrawImage(statusImage, new Point(xOffSet, yOffSet), new GraphicsOptions());
            xOffSet += statusLength + 2;
        }
       
        if (IsDead)
        {
            ctx.Opacity(0.5f);
        }

        ctx.EntropyCrop(0.05f);
     

        return image;
    }


}