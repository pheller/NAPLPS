using NAPLPS;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NAPLPSApp.Drawing;

/// <summary>Used on every drawable shape or command.</summary>
public interface IDrawable
{
    /// <summary></summary>
    /// <param name="image"></param>
    /// <param name="state"></param>
    /// <param name="size"></param>
    void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size);
}
