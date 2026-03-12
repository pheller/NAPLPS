// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace NAPLPS.Drawing;

public class DrawContext : IDisposable
{
    private bool disposedValue;

    private readonly MemoryStream memoryStream = new();

    // Track last displayed character for Repeat command
    private AsciiCharCommand? _lastDisplayedChar;

    /// <summary>Blink animator for palette animation. Initialized after render completes.</summary>
    public BlinkAnimator? BlinkAnimator { get; private set; }

    public NaplpsFormat NAPLPS { get; }

    public Size Size { get; }

    public Image<Rgba32> Image { get; }

    public event Action? OnImageUpdated;

    public uint CurrentIndex;

    public uint TotalFrames;

    /// <summary>
    /// When true, rendering uses the final parsed state's ColorMap as a live palette.
    /// All drawables resolve colors from this shared palette instead of their historical snapshots.
    /// This enables palette animation effects (blink, palette cycling) where previously-drawn
    /// objects retroactively change color when palette entries are modified.
    /// </summary>
    public bool PaletteAnimationMode { get; set; }

    public DrawContext() { }

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
        _lastDisplayedChar = null;

        // Clear canvas (important for loop restarts and re-renders)
        Image.Mutate(ctx => ctx.Fill(ISColor.Black));

        if (PaletteAnimationMode)
        {
            Drawable.LivePalette = NAPLPS.State.ColorMap;
        }

        foreach (var sequence in NAPLPS.Commands)
        {
            var (command, state) = sequence;

            // Handle scroll: shift image pixels up when scroll event occurs
            if (state.ScrollEventOccurred)
            {
                ScrollImageUp(state);
            }

            var drawable = ConvertToDrawable(command, state);

            // Track last displayed character for Repeat
            if (command is AsciiCharCommand asciiChar)
            {
                _lastDisplayedChar = asciiChar;
            }

            // Handle Repeat specially
            if (drawable is DrawableRepeat repeatDrawable)
            {
                RenderRepeat(repeatDrawable, state);
            }
            else
            {
                drawable?.Draw(Image, state, Size);
            }

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

        Drawable.LivePalette = null;

        OnImageUpdated?.Invoke();
    }

    public async Task RenderAsync(CancellationToken cancellationToken, uint delay)
    {
        CurrentIndex = 0;
        _lastDisplayedChar = null;

        // Clear canvas (important for loop restarts)
        Image.Mutate(ctx => ctx.Fill(ISColor.Black));

        if (PaletteAnimationMode)
        {
            Drawable.LivePalette = NAPLPS.State.ColorMap;
        }

        foreach (var sequence in NAPLPS.Commands)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var (command, state) = sequence;

            // Handle scroll: shift image pixels up when scroll event occurs
            if (state.ScrollEventOccurred)
            {
                ScrollImageUp(state);
            }

            var drawable = ConvertToDrawable(command, state);

            // Track last displayed character for Repeat
            if (command is AsciiCharCommand asciiChar)
            {
                _lastDisplayedChar = asciiChar;
            }

            // Handle Repeat specially
            if (drawable is DrawableRepeat repeatDrawable)
            {
                RenderRepeat(repeatDrawable, state);
            }
            else
            {
                drawable?.Draw(Image, state, Size);
            }

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

        Drawable.LivePalette = null;
    }

    private void RenderRepeat(DrawableRepeat repeatDrawable, NaplpsState state)
    {
        if (_lastDisplayedChar == null)
        {
            return;
        }

        var repeatCount = repeatDrawable.GetRepeatCount(state);
        var charToRepeat = _lastDisplayedChar.AsciiCharacter;

        // Create a temporary drawable for the character and draw it N times
        // We need to use the current state's pen position and advance it each time
        for (int i = 0; i < repeatCount; i++)
        {
            // Create a drawable for the repeated character using current pen position
            // Check DRCS table first
            IDrawable charDrawable;
            if (state.DrcsCharacters.TryGetValue(_lastDisplayedChar.OpCode, out var bitmap))
            {
                charDrawable = new DrawableDrcsChar(_lastDisplayedChar, bitmap);
            }
            else
            {
                charDrawable = new DrawableAsciiChar(_lastDisplayedChar);
            }
            charDrawable.Draw(Image, state, Size);

            // Advance pen position (same logic as AsciiCharCommand.MovePen)
            AdvancePenForCharacter(state, charToRepeat);
        }
    }

    private static void AdvancePenForCharacter(NaplpsState state, char character)
    {
        var pen = state.Pen;

        float spacingMultiplier = state.TextSpacing switch
        {
            TextSpacing.One => 1.0f,
            TextSpacing.FiveQuarters => 1.25f,
            TextSpacing.ThreeHalves => 1.5f,
            TextSpacing.Proportional => 1.0f,
            _ => 1.0f
        };

        float widthRatio = DrawableAsciiChar.GetCharWidthRatio(character);

        switch (state.TextPath)
        {
            case TextPath.Right:
            pen.X += state.CharSize.X * widthRatio * spacingMultiplier;
            break;
            case TextPath.Left:
            pen.X -= state.CharSize.X * widthRatio * spacingMultiplier;
            break;
            case TextPath.Up:
            pen.Y += state.CharSize.Y * spacingMultiplier;
            break;
            case TextPath.Down:
            pen.Y -= state.CharSize.Y * spacingMultiplier;
            break;
        }

        state.Pen = pen;
    }

    /// <summary>
    /// Shifts all pixels up by one text line height, clearing the bottom rows to black.
    /// Used when scroll mode is active and pen reaches below field origin.
    /// </summary>
    private void ScrollImageUp(NaplpsState state)
    {
        var (_, lineHeightPx) = ConvertNormalizedToScreenScale(Size, 0, state.CharSize.Y * GetInterrowMultiplier(state.TextInterrowSpacing));
        int shiftPixels = Math.Max(1, Math.Abs(lineHeightPx));

        Image.ProcessPixelRows(accessor =>
        {
            // Shift rows up
            for (int y = 0; y < accessor.Height - shiftPixels; y++)
            {
                var srcRow = accessor.GetRowSpan(y + shiftPixels);
                var dstRow = accessor.GetRowSpan(y);
                srcRow.CopyTo(dstRow);
            }

            // Clear bottom rows to black
            for (int y = accessor.Height - shiftPixels; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                row.Fill(new Rgba32(0, 0, 0, 255));
            }
        });
    }

    private static float GetInterrowMultiplier(TextInterrowSpacing spacing) => spacing switch
    {
        TextInterrowSpacing.One => 1.0f,
        TextInterrowSpacing.FiveQuarters => 1.25f,
        TextInterrowSpacing.ThreeHalves => 1.5f,
        TextInterrowSpacing.Two => 2.0f,
        _ => 1.0f
    };

    /// <summary>
    /// Initializes the blink animator after rendering completes.
    /// Call this if the parsed state has active blink processes.
    /// </summary>
    public void InitializeBlinkAnimator()
    {
        if (NAPLPS.State.BlinkProcesses.Count > 0)
        {
            BlinkAnimator = new BlinkAnimator(NAPLPS.State.BlinkProcesses, NAPLPS.State.ColorMap);
        }
    }

    /// <summary>
    /// Ticks the blink animator and re-renders if colors changed.
    /// Returns true if a re-render occurred.
    /// </summary>
    public bool TickBlink(int deltaMs)
    {
        if (BlinkAnimator == null || !BlinkAnimator.HasActiveProcesses)
        {
            return false;
        }

        bool changed = BlinkAnimator.Tick(deltaMs);

        if (changed)
        {
            // Re-render with palette animation mode to use the updated live palette
            var oldMode = PaletteAnimationMode;
            PaletteAnimationMode = true;
            Render();
            PaletteAnimationMode = oldMode;
            return true;
        }

        return false;
    }

    public void SaveAsPng(string filepath)
    {
        // TODO: Reset the image??
        Render();
        Image.SaveAsPng(filepath);
    }

    private static IDrawable? ConvertToDrawable(NaplpsCommand command, NaplpsState? state = null)
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

            case LineAbsoluteCommand:
            case LineRelativeCommand:
            case LineCommand lineCommand:
            {
                return new DrawableLine((LineCommand)command);
            }

            // Arc commands - all variants
            case ArcSetFilledCommand:
            case ArcSetOutlinedCommand:
            case ArcFilledCommand:
            case ArcOutlinedCommand:
            {
                return new DrawableArc((ArcCommand)command);
            }

            case ResetCommand resetCommand:
            {
                return new DrawableResetCommand(resetCommand);
            }

            case AsciiCharCommand asciiCharCommand:
            {
                // Check if this character has a DRCS replacement
                if (state != null && state.DrcsCharacters.TryGetValue(asciiCharCommand.OpCode, out var bitmap))
                {
                    return new DrawableDrcsChar(asciiCharCommand, bitmap);
                }
                return new DrawableAsciiChar(asciiCharCommand);
            }

            case PointCommand pointCommand:
            {
                return new DrawablePoint(pointCommand);
            }

            // Incremental commands
            case IncrementalPointCommand incPointCommand:
            {
                return new DrawableIncrementalPoint(incPointCommand);
            }

            case IncrementalLineCommand incLineCommand:
            {
                return new DrawableIncrementalLine(incLineCommand);
            }

            case IncrementalPolygonFilledCommand incPolyCommand:
            {
                return new DrawableIncrementalPolygonFilled(incPolyCommand);
            }

            case ControlCommand controlCommand when
                controlCommand.Command == NaplpsControlCommands.Repeat ||
                controlCommand.Command == NaplpsControlCommands.RepeatToEOL:
            {
                return new DrawableRepeat(controlCommand);
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
