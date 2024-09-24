using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using BasicFunctionality;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PublicInfo;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Entities.LegendaryBot.Entities.Items;



public abstract class Item : IInventoryEntity
{
    private static readonly ConcurrentDictionary<int, Item> _cachedDefaultItemsTypeIds = [];

    [Timestamp] public uint Version { get; private set; }

    public int Stacks { get; set; } = 1;

    public bool CanBeTraded => false;


    public abstract int TypeId { get; protected init; }
    public Type TypeGroup => typeof(Item);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public virtual string Description => string.Empty;
    public virtual Rarity Rarity => Rarity.OneStar;

    public abstract string Name { get; }
    public UserData? UserData { get; set; }
    public string ImageUrl => $"{Information.ItemsImagesDirectory}/{TypeId}.png";


    public long UserDataId { get; set; }

    public static Item GetDefaultFromTypeId(int typeId)
    {
        return _cachedDefaultItemsTypeIds.GetOrAdd(typeId,
            i =>
            {
                return TypesFunction.GetDefaultObjectsAndSubclasses<Item>()
                           .FirstOrDefault(j => j.TypeId == i)
                       ?? throw new Exception($"Item with type id {i} not found");
            });
    }


    public IInventoryEntity Clone()
    {
        var clone = (Item)MemberwiseClone();

        clone.UserData = null;
        clone.UserDataId = 0;
        return clone;
    }


    public async Task<Image<Rgba32>> GetImageAsync(int? stacks = null)
    {
        if (stacks is null)
            stacks = Stacks;
        var url = ImageUrl;


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

            ctx.Fill(Color.Black, new RectangleF(0, 0, size + xOffset, size));
            var font = SystemFonts.CreateFont(Information.GlobalFontName, size, FontStyle.Bold);

            ctx.DrawText(stacksString, font, Color.White, new PointF(x, 0));
        });

        return image;
    }
}