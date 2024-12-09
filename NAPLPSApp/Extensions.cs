using Avalonia.Media.Imaging;
using NAPLPS.Drawing;
using SixLabors.ImageSharp.Formats.Bmp;
using System.IO;

namespace NAPLPSApp;

public static class Extensions
{
    public static Bitmap ToBitmap(this DrawContext ctx)
    {
        using var memoryStream = new MemoryStream();

        ctx.Image.Save(memoryStream, new BmpEncoder());

        memoryStream.Position = 0;

        return new Bitmap(memoryStream);
    }
}
