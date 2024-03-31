using NAPLPS;
using NAPLPS.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NAPLPSApp.Drawing;

public class DrawContext : IDisposable
{
    private bool disposedValue;

    public NaplpsFormat NAPLPS { get; }

    public System.Drawing.Size Size { get; }

    public Image<Rgba32> Image { get;  }

    public DrawContext(NaplpsFormat naplps, System.Drawing.Size size)
    {
        NAPLPS = naplps ?? throw new ArgumentNullException(nameof(naplps));
        Size = size;
        Image = new(Size.Width, Size.Height);
    }

    public void SaveAsPng(string filepath)
    {
        // TODO: Reset the image??

        NAPLPS.Commands.ToList().ForEach(sequence =>
        {
            var (command, state) = sequence;

            var drawable = ConvertToDrawable(command);

            drawable?.Draw(Image, state, Size);
        });

        Image.SaveAsPng($"{filepath}.png");
    }

    private static IDrawable ConvertToDrawable(NaplpsCommand command)
    {
        switch (command)
        {
            case PolygonSetFilledCommand polygonCommand:
            {
                return new DrawablePolygonSetFilled(polygonCommand);
            }

            case LineRelativeCommand lineCommand:
            {
                return new DrawableLineRelative(lineCommand);
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
