using Microsoft.Extensions.Caching.Memory;
using PublicInfo;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BasicFunctionality;

public static class ImageFunctions
{
    private static readonly MemoryCacheEntryOptions EntryOptions = new()
    {
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration { EvictionCallback = DisposeEvictionCallback } }
    };

    private static readonly MemoryCacheEntryOptions ExpiryEntryOptions = new()
    {
        SlidingExpiration = new TimeSpan(0, 0, 30),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration { EvictionCallback = DisposeEvictionCallback } }
    };


    private static MemoryCache CachedImages { get; } = new(new MemoryCacheOptions());

    /// <summary>
    ///     Gets the image as sixlabors image from the url provided, caches it, and returns it
    /// </summary>
    /// <returns></returns>
    public static async Task<Image<Rgba32>> GetImageFromUrlAsync(string url)
    {
        if (CachedImages.TryGetValue(url, out Image<Rgba32>? gottenImage)) return gottenImage!.Clone();
        try
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    if (cert is not null && !cert.Verify())
                        return httpRequestMessage.RequestUri is not null
                               && httpRequestMessage.RequestUri.ToString().Contains(Information.ApiDomainName);
                    return cert is not null;
                };
            using var webClient = new HttpClient(handler);
            await using var memoryStream = await webClient.GetStreamAsync(url);
            var characterImage = await Image.LoadAsync<Rgba32>(memoryStream);
            var entryOptions = EntryOptions;

            if (!url.Contains(Information.ApiDomainName)) entryOptions = ExpiryEntryOptions;

            CachedImages.Set(url, characterImage, entryOptions);
            return characterImage.Clone();
        }
        catch (HttpRequestException)
        {
            var alternateImage =
                await GetImageFromUrlAsync($"{Information.BattleImagesDirectory}/dead_x.png");
            CachedImages.Set(url, alternateImage, EntryOptions);
            return alternateImage.Clone();
        }
    }

    public static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext context, Size size,
        float cornerRadius)
    {
        var temp = context.Resize(new ResizeOptions
        {
            Size = size,
            Mode = ResizeMode.Crop
        }).ApplyRoundedCorners(cornerRadius);
        context.SetGraphicsOptions(new GraphicsOptions());
        return temp;
    }

    public static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext context)
    {
        var contextSize = context.GetCurrentSize();
        return context.ConvertToAvatar(contextSize, contextSize.Height / 2.0f);
    }

    // This method can be seen as an inline implementation of an `IImageProcessor`:
    // (The combination of `IImageOperations.Apply()` + this could be replaced with an `IImageProcessor`)
    private static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext context, float cornerRadius)
    {
        var size = context.GetCurrentSize();
        var corners = BuildCorners(size.Width, size.Height, cornerRadius);

        context.SetGraphicsOptions(new GraphicsOptions
        {
            Antialias = true,

            // Enforces that any part of this shape that has color is punched out of the background
            AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
        });

        // Mutating in here as we already have a cloned original
        // use any color (not Transparent), so the corners will be clipped
        foreach (var path in corners) context = context.Fill(Color.Red, path);

        return context;
    }

    private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
    {
        // First create a square
        var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

        // Then cut out of the square a circle so we are left with a corner
        var cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

        // Corner is now a corner shape positions top left
        // let's make 3 more positioned correctly, we can do that by translating the original around the center of the image.

        var rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
        var bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

        // Move it across the width of the image - the width of the shape
        var cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
        var cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
        var cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

        return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
    }

    public static async void DisposeEvictionCallback(object key, object? value, EvictionReason reason, object? state)
    {
        if (value is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else if (value is IDisposable disposable) disposable.Dispose();
    }
}