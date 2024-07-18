using System.Collections.Concurrent;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
        

  


    /// <summary>
    /// Caches the cropped combat images, since cropping takes time
    /// </summary>
    private static ConcurrentDictionary<string,Image<Rgba32>> _cachedCombatCroppedImages = new();


    public async Task<Image<Rgba32>> GetImageForCombatAsync()
    {

        var image = new Image<Rgba32>(190, 150);
        var url = ImageUrl;
        if (!_cachedCombatCroppedImages.TryGetValue(url, out var characterImage))
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
        var width = 175;
        var shieldPercentage = ShieldPercentage;
        var filledWidth = (width * healthPercentage / 100.0).Round();
        var filledShieldWidth = (width * shieldPercentage / 100).Round();
        var barHeight = 16; 
        if(healthPercentage < 100)
            ctx.Fill(SixLabors.ImageSharp.Color.Red, new Rectangle(0, 50, width, barHeight));
        ctx.Fill(SixLabors.ImageSharp.Color.Green, new Rectangle(0, 50, filledWidth, barHeight));
        var shieldXPosition =  filledWidth;
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

        var xOffSet = 0;
        var yOffSet = 50 + barHeight + 5;

        var moveLength = 25; 

        foreach (var i in MoveList)
        {
            //do not change size of the move image here.
            //do it in the method that gets the image
            using var moveImage = await i.GetImageForCombatAsync();
            ctx.DrawImage(moveImage, new Point(xOffSet, yOffSet), new GraphicsOptions());
            xOffSet += moveLength;
            var cooldown = 0;
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