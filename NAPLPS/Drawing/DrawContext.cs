// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Drawing;

public class DrawContext : IDisposable
{
    private bool disposedValue;

    private readonly MemoryStream memoryStream = new();

    public NaplpsFormat NAPLPS { get; }

    public Size Size { get; }

    public Image<Rgba32> Image { get; }

    public event Action? OnImageUpdated;

    public uint CurrentIndex;

    public uint TotalFrames;

    public DrawContext(NaplpsFormat naplps, Size size)
    {
        NAPLPS = naplps ?? throw new ArgumentNullException(nameof(naplps));
        Size = size;
        Image = new(Size.Width, Size.Height);
        CurrentIndex = 0;
        TotalFrames = (uint)NAPLPS.Commands.Count - 1;
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

        if (CurrentIndex >= TotalFrames)
        {
            CurrentIndex = TotalFrames;
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

        if (CurrentIndex >= TotalFrames)
        {
            CurrentIndex = TotalFrames;
        }
    }

    public void SaveAsPng(string filepath)
    {
        // TODO: Reset the image??
        Render();
        Image.SaveAsPng($"{filepath}.png");
    }

    private static IDrawable? ConvertToDrawable(NaplpsCommand command)
    {
        switch (command)
        {
            case PolygonCommand polygonCommand:
            {
                return new DrawablePolygon(polygonCommand);
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

            case LineSetAbsoluteCommand:
            case LineSetRelativeCommand:
            {
                return new DrawableLineSet((LineCommand)command);
            }

            case LineCommand lineCommand:
            {
                return new DrawableLine(lineCommand);
            }

            case ArcSetFilledCommand arcCommand:
            {
                return new DrawableArc(arcCommand);
            }

            case ResetCommand resetCommand:
            {
                return new DrawableResetCommand(resetCommand);
            }

            case ShiftInCommand shiftInCommand:
            {
                return new DrawableShiftInCommand(shiftInCommand);
            }

            case PointCommand pointCommand:
            {
                // NOOP
                return new DrawablePoint(pointCommand);
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
