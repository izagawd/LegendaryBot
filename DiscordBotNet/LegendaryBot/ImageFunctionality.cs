using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot;

public static class ImageFunctionality
{
        public static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext context, Size size, float cornerRadius)
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

            context.SetGraphicsOptions(new GraphicsOptions()
            {
                Antialias = true,

                // Enforces that any part of this shape that has color is punched out of the background
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
            });

            // Mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach (var path in corners)
            {
                context = context.Fill(Color.Red, path);
            }

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
}