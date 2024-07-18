using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class BlessingDatabaseConfiguration : IEntityTypeConfiguration<Blessing>
{
    public void Configure(EntityTypeBuilder<Blessing> builder)
    {
        builder.HasKey(i => i.Id);
    }
}
public abstract class Blessing : IInventoryEntity
{



    public virtual  string Description => GetDescription(Level);
    public abstract Rarity Rarity { get; }
    public IInventoryEntity Clone()
    {
        throw new NotImplementedException();
    }

    public string Name { get; }
    public UserData? UserData { get; set; }

    public virtual string GetDescription(int level)
    {
        return "Idk man";
    }
    public  Type TypeGroup => typeof(Blessing);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public Guid? CharacterId { get; set; }
    public virtual async Task<Image<Rgba32>> GetInfoAsync()
    {
        using var userImage = await BasicFunctionality.GetImageFromUrlAsync(ImageUrl);
        var image = new Image<Rgba32>(500, 150);
        userImage.Mutate(ctx => ctx.Resize(new Size(100,100)));
        var userImagePoint = new Point(20, 20);
   
        var levelBarY = userImage.Height - 30 + userImagePoint.Y;
        var font = SystemFonts.CreateFont(Bot.GlobalFontName, 25);
        var xPos = 135;
        image.Mutate(ctx =>
        
            ctx
                .DrawImage(userImage,userImagePoint, new GraphicsOptions())
                .Draw(Color.Black, 3, new RectangleF(userImagePoint,userImage.Size))
                .DrawText($"Name: {Name}", font, Color.Black, new PointF(xPos, levelBarY+2))

                .Resize(1000, 300));
        

        return image;
    }


    
    

    public string ImageUrl { get; }
    public Guid Id { get; set; }
    public ulong UserDataId { get; set; }


    [NotMapped] public virtual int Attack => 20 + (Level * 4);
    [NotMapped] public virtual int Health => 70 + (Level * 8);


    public Blessing()
    {
        ImageUrl = $"{Website.DomainName}/battle_images/blessings/{GetType().Name}.png";
    }
    [NotMapped]
    protected int Level => (Character?.Level).GetValueOrDefault(1);
    [NotMapped]
    public bool IsInStandardBanner => true;
    public Character? Character { get; set; }

    public IEnumerable<string> ImageUrls { get; }
}