using SkiaSharp;

public static class SKBitmapExtensions
{
    public static byte[] ToByteArray(this SKBitmap bitmap, SKEncodedImageFormat format) {
        using (var ms = new MemoryStream())
        {
            bitmap.Encode(format, 100).SaveTo(ms);

            byte[] byteArray = ms.ToArray();
            return byteArray;
        }
    }

    public static ImageSource ToImageSource(this SKBitmap bitmap)
    {
        SKPixmap pixmap = bitmap.PeekPixels();

        var info = new SKImageInfo(pixmap.Width, pixmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

        using (SKSurface surface = SKSurface.Create(info))
        {
            surface.Canvas.DrawBitmap(bitmap, 0, 0);
            using (SKImage skImage = surface.Snapshot())
            {
                using (SKData data = skImage.Encode())
                {
                    byte[] bytes = data.ToArray();
                    return ImageSource.FromStream(() => new MemoryStream(bytes));
                }
            }
        }
    }

    public static SKBitmap CreateCircularImage(this SKBitmap bitmap, int width, int height)
    {
        using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
        {
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            var path = new SKPath();
            path.AddCircle(width / 2f, height / 2f, Math.Min(width, height) / 2f);

            canvas.ClipPath(path);

            canvas.DrawBitmap(bitmap, new SKRect(0, 0, width, height));

            return SKBitmap.FromImage(surface.Snapshot());
        }
    }
}
