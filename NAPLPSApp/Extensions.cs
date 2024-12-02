// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

#if NET8_0_WINDOWS
using NAPLPS.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
#endif

using Size = System.Drawing.Size;

namespace NAPLPSApp;

public static class Extensions
{
    public static SixLabors.ImageSharp.Size ToISSize(this Size size) => new(size.Width, size.Height);

#if NET8_0_WINDOWS
    /// <summary>Converts the NAPLPS final image to a System.Drawing.Image</summary>
    /// <returns>A hopefully well drawn NAPLPS image in a System.Drawing.Image format</returns>
    public static System.Drawing.Image ToImage(this DrawContext ctx)
    {
        using var memoryStream = new MemoryStream();

        ctx.Image.Save(memoryStream, PngFormat.Instance);

        var image = System.Drawing.Image.FromStream(memoryStream);

        return image;
    }

    // Winforms Extensions
    public static string SizeString(this Size size) => $"{size.Width}x{size.Height}";

    public static Size StringSize(this string stringSize)
    {
        var sizeParsed = stringSize.Split("x");

        return new Size(int.Parse(sizeParsed[0]), int.Parse(sizeParsed[1]));
    }
#endif

}
