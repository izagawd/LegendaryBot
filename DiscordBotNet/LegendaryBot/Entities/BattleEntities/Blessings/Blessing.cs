using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Reflection;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Results;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public abstract class Blessing : BattleEntity
{
    public Guid? BlessingWielderId { get; set; }
    public virtual async Task<Image<Rgba32>> GetInfoAsync()
    {
        using var userImage = await BasicFunctionality.GetImageFromUrlAsync(ImageUrl);
        var image = new Image<Rgba32>(500, 150);
        userImage.Mutate(ctx => ctx.Resize(new Size(100,100)));
        var userImagePoint = new Point(20, 20);
        var levelBarMaxLevelWidth = 250L;
        var gottenExp = levelBarMaxLevelWidth * (Experience/(GetRequiredExperienceToNextLevel() * 1.0f));
        var levelBarY = userImage.Height - 30 + userImagePoint.Y;
        var font = SystemFonts.CreateFont(Bot.GlobalFontName, 25);
        var xPos = 135;
        image.Mutate(ctx =>
        
            ctx
                .DrawImage(userImage,userImagePoint, new GraphicsOptions())
                .Draw(Color.Black, 3, new RectangleF(userImagePoint,userImage.Size))
                .Fill(Color.Gray, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30))
                .Fill(Color.Green, new RectangleF(130, levelBarY, gottenExp, 30))
                .Draw(Color.Black, 3, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30))
                .DrawText($"{Experience}/{GetRequiredExperienceToNextLevel()}",font,Color.Black,new PointF(xPos,levelBarY+2))
                .DrawText($"Name: {Name}", font, Color.Black, new PointF(xPos, levelBarY -57))
                .DrawText($"Level: {Level}",font,Color.Black,new PointF(xPos,levelBarY - 30))
                .Resize(1000, 300));
        

        return image;
    }


    
    
    /// <summary>
    /// The description of the blessing in relation to the level provided
    /// </summary>

    public abstract string GetDescription(int level);

    public sealed override string Description => GetDescription(Level);
    public override string ImageUrl => $"{Website.DomainName}/battle_images/blessings/{GetType().Name}.png";

    public sealed  override int MaxLevel => 15;
    [NotMapped] public virtual int Attack => 200;
    [NotMapped] public virtual int Health => 200;


    public override async Task<Image<Rgba32>> GetDetailsImageAsync()
    {
        var image = new Image<Rgba32>(500, 350);
        using var blessingImage = await BasicFunctionality.GetImageFromUrlAsync(ImageUrl);
        blessingImage.Mutate(i => i.Resize(200,200));
        var drawing = new RichTextOptions(SystemFonts.CreateFont(Bot.GlobalFontName, 20));
        drawing.Origin = new Vector2(10, 200);
        drawing.WrappingLength = 490;
        image.Mutate(i => i
            .DrawImage(blessingImage,new Point((image.Width/2.0 - blessingImage.Size.Width/2.0).Round(),0),new GraphicsOptions())
            .DrawText(drawing,Description,Color.Black)
            .BackgroundColor(Color.Aqua));
       
        return image;


    }
    [NotMapped]
    public bool IsInStandardBanner => true;
    public Character? Character { get; set; }
    public override ExperienceGainResult IncreaseExp(long experienceToGain)
    {
        string expGainText = "";

        var levelBefore = Level;
        Experience += experienceToGain;
        var nextLevelEXP = BattleFunctionality.NextLevelFormula(Level);
        while (Experience >= nextLevelEXP &&  Level < MaxLevel)
        {
            Experience -= nextLevelEXP;
            Level += 1;
            nextLevelEXP = BattleFunctionality.NextLevelFormula(Level);
        }
        long excessExp = 0;
        if (Experience > nextLevelEXP)
        {
            excessExp = Experience - nextLevelEXP;
        }

        expGainText += $"{this} gained {experienceToGain} exp";
        if (levelBefore != Level)
        {
            expGainText += $", and moved from level {levelBefore} to level {Level}";
        }

        expGainText += "!";
        return new ExperienceGainResult() { Text = expGainText, ExcessExperience = excessExp };

    }
}