using DiscordBotNet.LegendaryBot.StatusEffects;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.Items;



public static class ItemExtensions
{

    public static IEnumerable<TItem> MergeItems<TItem>(this IEnumerable<TItem> items) where TItem : Item
    {
        Dictionary<Type, TItem> dictionary = new();
        foreach (var i in items)
        {
            if(i.Stacks <= 0) continue;
            if (!dictionary.ContainsKey(i.GetType()))
            {
                dictionary[i.GetType()] = i;
            }
            else
            {
                dictionary[i.GetType()].Stacks += i.Stacks;
            }
        }
        foreach (var i in dictionary.Values)
        {
            yield return i;
        }
    }
}
public abstract class Item : Entity
{
    public override string ImageUrl => $"{Website.DomainName}/battle_images/items/bruh.png";

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
                xOffset = 3 * (theMin/25.0f);
            }
            ctx.Fill(Color.Black, new RectangleF(0, 0, size+xOffset, size));
            var font = SystemFonts.CreateFont(Bot.GlobalFontName, size, FontStyle.Bold);
 
            ctx.DrawText(stacksString, font, Color.White, new PointF(x, 0));
        });

        return image;
    }


    public int Stacks { get; set; } = 1;






}