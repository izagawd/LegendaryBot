using DiscordBotNet.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.Items;

public class ItemDatabaseConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(nameof(Item.UserDataId),"Discriminator");
    }
}


public abstract class Item : IInventoryEntity
{
    public  Type TypeGroup => typeof(Item);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public string Description { get; }
    public virtual Rarity Rarity { get; }


    public IInventoryEntity Clone()
    {
        var clone =(Item)  MemberwiseClone();
       
        clone.UserData = null;
        clone.UserDataId = 0;
        return clone;
    }

    public string Name { get; }
    public UserData? UserData { get; set; }
    public  string ImageUrl { get; }

    public Item()
    {
        ImageUrl = $"{Website.DomainName}/battle_images/items/{GetType().Name}.png";
        Name = BasicFunctionality.Englishify(GetType().Name);
    }

    public ulong UserDataId { get; set; }


    public async Task<Image<Rgba32>> GetImageAsync(int? stacks = null)
    {
        if (stacks is null)
            stacks = Stacks;
        var url = ImageUrl;


        var image = await BasicFunctionality.GetImageFromUrlAsync(url);

        var theMin =Math.Min(image.Width, image.Height);
        image.Mutate(i => i.Resize(new Size(theMin,theMin)) );
        image.Mutate(ctx =>
        {
            var size = 9 * (theMin/25.0f);
            var x = 1 * (theMin/25.0f);
            float xOffset = 0;
            var stacksString = stacks.ToString();
            if (stacksString.Length > 1)
            {
                x = 0;
                xOffset = (stacksString.Length - 1)* 3 * (theMin/25.0f);
            }
            ctx.Fill(Color.Black, new RectangleF(0, 0, size+xOffset, size));
            var font = SystemFonts.CreateFont(Bot.GlobalFontName, size, FontStyle.Bold);
 
            ctx.DrawText(stacksString, font, Color.White, new PointF(x, 0));
        });

        return image;
    }


    public int Stacks { get; set; } = 1;



}