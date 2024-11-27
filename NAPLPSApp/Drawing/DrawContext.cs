// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NAPLPSApp.Drawing;

public class DrawContext : IDisposable
{
    private bool disposedValue;

    private readonly MemoryStream memoryStream = new();

    public NaplpsFormat NAPLPS { get; }

    public System.Drawing.Size Size { get; }

    public Image<Rgba32> Image { get; }

    public event Action? OnImageUpdated;

    public uint CurrentIndex;

    public uint TotalFrames;

    public DrawContext(NaplpsFormat naplps, System.Drawing.Size size)
    {
        NAPLPS = naplps ?? throw new ArgumentNullException(nameof(naplps));
        Size = size;
        Image = new(Size.Width, Size.Height);
        CurrentIndex = 0;
        TotalFrames = (uint)NAPLPS.Commands.Count;
    }

    public void Render(uint sequenceNumber = uint.MaxValue)
    {
        CurrentIndex = 0;

        foreach (var sequence in NAPLPS.Commands)
        {
            var (command, state) = sequence;

            var drawable = ConvertToDrawable(command);

            drawable?.Draw(Image, state, Size);

            if (CurrentIndex == sequenceNumber)
            {
                break;
            }

            CurrentIndex++;
        }


        OnImageUpdated?.Invoke();
    }

    public async Task RenderAsync(CancellationToken cancellationToken, uint delay)
    {
        CurrentIndex = 0;

        foreach (var sequence in NAPLPS.Commands)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var (command, state) = sequence;

            var drawable = ConvertToDrawable(command);

            drawable?.Draw(Image, state, Size);

            OnImageUpdated?.Invoke();

            CurrentIndex++;

            if (drawable != null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken); // TODO: Calculate the delay
            }
        }

        CurrentIndex = TotalFrames;
    }

#if NET8_0_WINDOWS
    /// <summary>Converts the NAPLPS final image to a System.Drawing.Image</summary>
    /// <returns>A hopefully well drawn NAPLPS image in a System.Drawing.Image format</returns>
    public System.Drawing.Image ToImage()
    {
        memoryStream.SetLength(0);

        Image.Save(memoryStream, PngFormat.Instance);

        var image = System.Drawing.Image.FromStream(memoryStream);

        image.RotateFlip(RotateFlipType.Rotate180FlipX);

        return image;
    }
#endif

    public void SaveAsPng(string filepath)
    {
        // TODO: Reset the image??

        this.Render();
        this.Image.Mutate(x => x.Flip(FlipMode.Vertical));
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

            case ArcSetFilledCommand arcCommand:
            {
                return new DrawableArcSetFilled(arcCommand);
            }

            case ResetCommand resetCommand:
            {
                return new DrawableResetCommand(resetCommand);
            }

            case ShiftInCommand shiftInCommand:
            {
                return new DrawableShiftInCommand(shiftInCommand);
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
                memoryStream.Dispose();

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
