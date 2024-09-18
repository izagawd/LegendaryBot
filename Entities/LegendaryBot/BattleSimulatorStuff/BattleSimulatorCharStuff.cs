
using System.Collections.Concurrent;
using BasicFunctionality;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Moves;
using PublicInfo;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Entities.LegendaryBot.BattleSimulatorStuff;

public partial class BattleSimulator
{
        /// <summary>
    ///     Caches the cropped combat images, since cropping takes time
    /// </summary>
    private static readonly ConcurrentDictionary<string, Image<Rgba32>> CachedCharacterCombatCroppedImages = new();


    protected static ConcurrentDictionary<string, Image<Rgba32>> CachdMoveCroppedCombatImages { get; } = new();


    public async Task<Image<Rgba32>> GenerateMovesCombatImageAsync(Move move)
    {
    
        var url = move.IconUrl;
        if (!CachdMoveCroppedCombatImages.TryGetValue(url, out var image))
        {
            image = await ImageFunctions.GetImageFromUrlAsync(url);
            image.Mutate(i => i
                .Resize(25 , 25 )
                .Draw(Color.Black, 3 , new RectangleF(0, 0, 24 ,
                    24)));
            CachdMoveCroppedCombatImages[url] = image;
        }


        return image;
        

    }
    public async Task<Image<Rgba32>> GenerateCharacterDetailsImageForCombatAsync(Character character)
    {
        const int characterImageSize = 50;
        var image = new Image<Rgba32>(190, 150);
        var url = character.ImageUrl;
        if (!CachedCharacterCombatCroppedImages.TryGetValue(url, out var characterImage))
        {
            characterImage = await ImageFunctions.GetImageFromUrlAsync(url);
            characterImage
                .Mutate(ctx => { ctx.Resize(new Size(characterImageSize, characterImageSize)); });
            //any image outside of the domain will n=be removed after a certain amount of time using this entry option
            CachedCharacterCombatCroppedImages[url] = characterImage;
        }

        IImageProcessingContext ctx = null!;
        image.Mutate(idk => ctx = idk);

        

        ctx
            .DrawImage(characterImage, new Point(0, 0), new GraphicsOptions());
            
        
        if(character.IsDead)
        {
            
            using var gottenImage =
                await ImageFunctions.GetImageFromUrlAsync(PublicInfo.Information.BattleImagesDirectory + "/dead_x.png");
            gottenImage.Mutate(i => i.Resize(new Size(characterImageSize,characterImageSize)));
            ctx
                .DrawImage(gottenImage, new Point(0, 0), new GraphicsOptions());
        }   
         ctx
            .Draw(Color.Black, 1, new Rectangle(new Point(0, 0), new Size(50, 50)))
            .DrawText($"Lvl {character.Level}", SystemFonts.CreateFont(Information.GlobalFontName, 10),
                Color.Black, new PointF(55, 21.5f))
            .Draw(Color.Black, 1,
                new RectangleF(52.5f, 20, 70, 11.5f))
            .DrawText(character.Name + $" [{character.AlphabetIdentifier}] [{character.Position}]",
                SystemFonts.CreateFont(Information.GlobalFontName, 11),
                Color.Black, new PointF(55, 36.2f))
            .Draw(Color.Black, 1,
                new RectangleF(52.5f, 35, 115, 12.5f));
        if (character.UsesSuperPoints)
            ctx
                .DrawText($"SP: {character.SuperPoints}", SystemFonts.CreateFont(Information.GlobalFontName, 10),
                    Color.Black, new PointF(55, 6.5f))
                .Draw(Color.Black, 1,
                    new RectangleF(52.5f, 5f, 40, 11.5f));
        var healthPercentage = character.HealthPercentage;
        const int width = 175;
        var shieldPercentage = character.ShieldPercentage;
        var filledWidth = (width * healthPercentage / 100.0).Round();
        var filledShieldWidth = (width * shieldPercentage / 100).Round();
        const int barHeight = 16;
        if (healthPercentage < 100)
            ctx.Fill(Color.Red, new Rectangle(0, 50, width, barHeight));
        ctx.Fill(Color.Green, new Rectangle(0, 50, filledWidth, barHeight));
        var shieldXPosition = filledWidth;
        if (shieldXPosition + filledShieldWidth > width) shieldXPosition = width - filledShieldWidth;
        if (shieldPercentage > 0)
            ctx.Fill(Color.White,
                new RectangleF(shieldXPosition, 50, filledShieldWidth, barHeight));

        // Creates a border for the health bar
        ctx.Draw(Color.Black, 0.5f, new Rectangle(0, 50, width, barHeight));
        ctx.DrawText($"{character.Health.Round()}/{character.MaxHealth.Round()}", SystemFonts.CreateFont(Information.GlobalFontName, 14),
            Color.Black, new PointF(2.5f, 51.5f));

        var xOffSet = 0;
        var yOffSet = 50 + barHeight + 5;

        const int moveLength = 25;

        foreach (var i in character.MoveList.Take(3))
        {
            //do not change size of the move image here.
            //do it in the method that gets the image
            var moveImage = await GenerateMovesCombatImageAsync(i);
            ctx.DrawImage(moveImage, new Point(xOffSet, yOffSet), new GraphicsOptions());
            xOffSet += moveLength;
            var cooldown = 0;
            if (i is Special special) cooldown = special.Cooldown;

            var cooldownString = "";
            if (cooldown > 0) cooldownString = cooldown.ToString();
            ctx.DrawText(cooldownString, SystemFonts.CreateFont(Information.GlobalFontName, moveLength),
                Color.Black, new PointF(xOffSet + 5, yOffSet));
            xOffSet += moveLength;
        }


        xOffSet = 0;
        yOffSet += moveLength + 5;


        foreach (var i in character.StatusEffects.Take(16))
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

        if (character != ActiveCharacter)
        {
            ctx.Opacity(0.6f);
        }


        
        ctx.EntropyCrop(0.05f);


        return image;
    }
    
}