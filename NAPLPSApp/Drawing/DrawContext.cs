// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace NAPLPSApp.Drawing;

public class DrawContext : IDisposable
{
    private bool disposedValue;

    public NaplpsFormat NAPLPS { get; }

    public System.Drawing.Size Size { get; }

    public Image<Rgba32> Image { get; }

    public DrawContext(NaplpsFormat naplps, System.Drawing.Size size)
    {
        NAPLPS = naplps ?? throw new ArgumentNullException(nameof(naplps));
        Size = size;
        Image = new(Size.Width, Size.Height);

        Render();
    }

    public void Render()
    {
        NAPLPS.Commands.ToList().ForEach(sequence =>
        {
            var (command, state) = sequence;

            var drawable = ConvertToDrawable(command);

            drawable?.Draw(Image, state, Size);
        });
    }

#if NET8_0_WINDOWS
    /// <summary>Converts the NAPLPS final image to a System.Drawing.Image</summary>
    /// <returns>A hopefully well drawn NAPLPS image in a System.Drawing.Image format</returns>
    public System.Drawing.Image ToImage()
    {
        using var ms = new MemoryStream();

        Image.Save(ms, PngFormat.Instance);

        using var msi = new MemoryStream(ms.ToArray());

        return System.Drawing.Image.FromStream(ms);
    }
#endif

    public void SaveAsPng(string filepath)
    {
        // TODO: Reset the image??

        Image.SaveAsPng($"{filepath}.png");
    }

    private static IDrawable? ConvertToDrawable(NaplpsCommand command)
    {
        switch (command)
        {
            case PolygonSetFilledCommand polygonCommand:
            {
                return new DrawablePolygonSetFilled(polygonCommand);
            }

            case PolygonSetOutlinedCommand polygonCommand:
            {
                return new DrawablePolygonSetOutlined(polygonCommand);
            }

            case PolygonFilledCommand polygonCommand:
            {
                return new DrawablePolygonFilled(polygonCommand);
            }

            case PolygonOutlinedCommand polygonCommand:
            {
                return new DrawablePolygonOutlined(polygonCommand);
            }

            case RectangleSetFilledCommand rectangleCommand:
            {
                return new DrawableRectangleSetFilled(rectangleCommand);
            }

            case RectangleSetOutlinedCommand rectangleCommand:
            {
                return new DrawableRectangleSetOutlined(rectangleCommand);
            }

            case RectangleFilledCommand rectangleCommand:
            {
                return new DrawableRectangleFilled(rectangleCommand);
            }

            case RectangleOutlinedCommand rectangleCommand:
            {
                return new DrawableRectangleOutlined(rectangleCommand);
            }

            case LineSetRelativeCommand lineCommand:
            {
                return new DrawableLineSetRelative(lineCommand);
            }

            case LineSetAbsoluteCommand lineCommand:
            {
                return new DrawableLineSetAbsolute(lineCommand);
            }

            case LineRelativeCommand lineCommand:
            {
                return new DrawableLineRelative(lineCommand);
            }

            case LineAbsoluteCommand lineCommand:
            {
                return new DrawableLineAbsolute(lineCommand);
            }

            case PointSetAbsoluteCommand:
            case PointSetRelativeCommand:
            {
                // NOOP
                return null;
            }

            default:
            {
                return null;
            }
        }
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state here (managed objects)
                Image.Dispose();
            }

            // Free unmanaged resources here (unmanaged objects) and override finalizer
            // Also set large fields to null!
            disposedValue = true;
        }
    }

    // ~DrawContext()
    // {
    //     // Only use this if we're freeing unmanaged resources...
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        Dispose(disposing: true);

        GC.SuppressFinalize(this);
    }

    #endregion
}
