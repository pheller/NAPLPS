// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
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

    public uint CurrentIndex { get; set; }

    public uint TotalFrames { get; set; }

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
        BeginRender();

        foreach (var (command, state) in NAPLPS.Commands)
        {
            RenderCommand(command, state);

            if (CurrentIndex == sequenceNumber)
            {
                break;
            }

            CurrentIndex++;
        }

        EndRender();
        OnImageUpdated?.Invoke();
    }

    public async Task RenderAsync(CancellationToken cancellationToken, uint delay)
    {
        BeginRender();

        foreach (var sequence in NAPLPS.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (command, state) = sequence;
            var drawable = RenderCommand(command, state);

            OnImageUpdated?.Invoke();
            CurrentIndex++;

            if (drawable != null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
            }
        }

        EndRender();
    }

    private void BeginRender()
    {
        CurrentIndex = 0;
        _lastDisplayedChar = null;

        // Clear canvas (important for loop restarts and re-renders)
        Image.Mutate(ctx => ctx.Fill(ISColor.Black));

        // NAPLPS is a CLUT system: palette changes via SET COLOR retroactively affect
        // all previously drawn objects using that palette index. Always use the final
        // parsed state's palette for color resolution in modes 1 and 2.
        Drawable.LivePalette = NAPLPS.State.ColorMap;
    }

    private void EndRender()
    {
        if (CurrentIndex >= TotalFrames)
        {
            CurrentIndex = TotalFrames;
        }

        Drawable.LivePalette = null;
    }

    /// <summary>
    /// Renders a single command. Returns the drawable if one was created (for delay timing).
    /// </summary>
    private IDrawable? RenderCommand(NaplpsCommand command, NaplpsState state)
    {
        // Handle scroll: shift image pixels up when scroll event occurs
        if (state.ScrollEventOccurred)
        {
            ScrollImageUp(state);
        }

        var drawable = ConvertToDrawable(command, state);

        // Track last displayed character for Repeat (skip discarded chars from word wrap)
        if (command is AsciiCharCommand asciiChar && !asciiChar.IsDiscarded)
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

        return drawable;
    }

    private void RenderRepeat(DrawableRepeat repeatDrawable, NaplpsState state)
    {
        if (_lastDisplayedChar == null)
        {
            return;
        }

        // ANSI X3.110: REPEAT can only repeat spacing characters from the ASCII,
        // supplementary, DRCS, or mosaic sets. Non-spacing accents cannot be repeated.
        // If the preceding character is not allowed, REPEAT is discarded.
        if (_lastDisplayedChar.IsNonSpacing)
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

        {
            float spacingMultiplier = state.TextSpacing switch
            {
                TextSpacing.One => 1.0f,
                TextSpacing.FiveQuarters => 1.25f,
                TextSpacing.ThreeHalves => 1.5f,
                _ => 1.0f
            };

            float widthRatio = DrawableAsciiChar.GetCharWidthRatio(character);

            switch (state.TextPath)
            {
                case TextPath.Right:
                {
                    pen.X += state.CharSize.X * widthRatio * spacingMultiplier;
                }
                break;

                case TextPath.Left:
                {
                    pen.X -= state.CharSize.X * widthRatio * spacingMultiplier;
                }
                break;

                case TextPath.Up:
                {
                    pen.Y += state.CharSize.Y * spacingMultiplier;
                }
                break;

                case TextPath.Down:
                {
                    pen.Y -= state.CharSize.Y * spacingMultiplier;
                }
                break;
            }
        }

        state.Pen = pen;
    }

    /// <summary>
    /// ANSI X3.110: Scrolls pixels up by one text line height.
    /// If the cursor is within the active field, only the field region scrolls.
    /// Otherwise, the entire display scrolls.
    /// </summary>
    private void ScrollImageUp(NaplpsState state)
    {
        var (_, lineHeightPx) = ConvertNormalizedToScreenScale(Size, 0, state.CharSize.Y * GetInterrowMultiplier(state.TextInterrowSpacing));
        int shiftPixels = Math.Max(1, Math.Abs(lineHeightPx));

        // Determine scroll region: active field if pen is inside it, otherwise full screen
        bool fieldScoped = state.Field.Dimensions.X > 0 && state.Field.Dimensions.Y > 0;

        if (fieldScoped)
        {
            var fieldOrigin = ConvertNormalizedToPoint(Size, state.Field.Origin.X, state.Field.Origin.Y + state.Field.Dimensions.Y);
            var fieldEnd = ConvertNormalizedToPoint(Size, state.Field.Origin.X + state.Field.Dimensions.X, state.Field.Origin.Y);
            int left = Math.Max(0, (int)fieldOrigin.X);
            int top = Math.Max(0, (int)fieldOrigin.Y);
            int right = Math.Min(Image.Width, (int)fieldEnd.X);
            int bottom = Math.Min(Image.Height, (int)fieldEnd.Y);

            Image.ProcessPixelRows(accessor =>
            {
                for (int y = top; y < bottom - shiftPixels; y++)
                {
                    var srcRow = accessor.GetRowSpan(y + shiftPixels);
                    var dstRow = accessor.GetRowSpan(y);
                    srcRow.Slice(left, right - left).CopyTo(dstRow.Slice(left, right - left));
                }

                for (int y = Math.Max(top, bottom - shiftPixels); y < bottom; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    row.Slice(left, right - left).Fill(new Rgba32(0, 0, 0, 255));
                }
            });
        }
        else
        {
            Image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height - shiftPixels; y++)
                {
                    var srcRow = accessor.GetRowSpan(y + shiftPixels);
                    var dstRow = accessor.GetRowSpan(y);
                    srcRow.CopyTo(dstRow);
                }

                for (int y = accessor.Height - shiftPixels; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    row.Fill(new Rgba32(0, 0, 0, 255));
                }
            });
        }
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
        Render();
        Image.SaveAsPng(filepath);
    }

    internal static IDrawable? ConvertToDrawable(NaplpsCommand command, NaplpsState? state = null)
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
                // Discarded characters (trailing spaces in word wrap) produce no drawable
                if (asciiCharCommand.IsDiscarded)
                {
                    return null;
                }

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

            case MosaicElementCommand mosaicCommand:
            {
                return new DrawableMosaicElement(mosaicCommand);
            }

            case ControlCommand controlCommand when
                controlCommand.Command == NaplpsControlCommands.ClearScreen:
            {
                return new DrawableClearScreen(controlCommand);
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

    /// <summary>
    /// Renders all command frames into a deduped APNG. Identical consecutive frames
    /// are collapsed into a single frame with an extended delay.
    /// Caller owns the returned image and must dispose it.
    /// </summary>
    /// <param name="delayHundredths">Base frame delay in hundredths of a second (default 5 = 50ms)</param>
    public Image<Rgba32> RenderToApng(int delayHundredths = 5, bool loop = false)
    {
        var apng = new Image<Rgba32>(Size.Width, Size.Height);
        var pngMeta = apng.Metadata.GetFormatMetadata(PngFormat.Instance);
        pngMeta.RepeatCount = loop ? (uint)0 : 1;

        var baseDelay = new Rational((uint)delayHundredths, 1000);
        Image<Rgba32>? previousFrame = null;
        int currentFrameDelayMultiplier = 1;

        // Incremental rendering: single pass through commands, drawing each on top
        // of the existing canvas. O(n) instead of O(n²). Safe because LivePalette
        // uses the final parsed palette — color resolution is order-independent.
        BeginRender();

        foreach (var sequence in NAPLPS.Commands)
        {
            var (cmd, cmdState) = sequence;
            var drawable = RenderCommand(cmd, cmdState);

            // Only check for frame changes when something was actually drawn
            if (drawable != null)
            {
                if (previousFrame != null && FramesAreIdentical(Image, previousFrame))
                {
                    currentFrameDelayMultiplier++;
                }
                else
                {
                    if (previousFrame != null)
                    {
                        AddApngFrame(apng, previousFrame, baseDelay, currentFrameDelayMultiplier);
                        previousFrame.Dispose();
                    }

                    previousFrame = Image.Clone();
                    currentFrameDelayMultiplier = 1;
                }
            }
            else
            {
                currentFrameDelayMultiplier++;
            }

            CurrentIndex++;
        }

        if (previousFrame != null)
        {
            AddApngFrame(apng, previousFrame, baseDelay, currentFrameDelayMultiplier);
            previousFrame.Dispose();
        }

        Drawable.LivePalette = null;

        return apng;
    }

    private static void AddApngFrame(Image<Rgba32> apng, Image<Rgba32> frame, Rational baseDelay, int delayMultiplier)
    {
        var frameDelay = new Rational(baseDelay.Numerator * (uint)delayMultiplier, baseDelay.Denominator);

        if (apng.Frames.Count == 1 && IsBlankFrame(apng.Frames.RootFrame))
        {
            apng.Frames.RootFrame.ProcessPixelRows(frame.Frames.RootFrame, (dst, src) =>
            {
                for (int y = 0; y < dst.Height; y++)
                {
                    src.GetRowSpan(y).CopyTo(dst.GetRowSpan(y));
                }
            });

            apng.Frames.RootFrame.Metadata.GetFormatMetadata(PngFormat.Instance).FrameDelay = frameDelay;
        }
        else
        {
            var added = apng.Frames.AddFrame(frame.Frames.RootFrame);
            added.Metadata.GetFormatMetadata(PngFormat.Instance).FrameDelay = frameDelay;
        }
    }

    private static bool IsBlankFrame(ImageFrame<Rgba32> frame)
    {
        bool allBlack = true;

        frame.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height && allBlack; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    if (row[x].R != 0 || row[x].G != 0 || row[x].B != 0 || row[x].A != 255)
                    {
                        allBlack = false;
                        break;
                    }
                }
            }
        });

        return allBlack;
    }

    private static bool FramesAreIdentical(Image<Rgba32> a, Image<Rgba32> b)
    {
        if (a.Width != b.Width || a.Height != b.Height)
        {
            return false;
        }

        bool identical = true;

        a.ProcessPixelRows(b, (accessorA, accessorB) =>
        {
            for (int y = 0; y < accessorA.Height && identical; y++)
            {
                var rowA = accessorA.GetRowSpan(y);
                var rowB = accessorB.GetRowSpan(y);

                if (!rowA.SequenceEqual(rowB))
                {
                    identical = false;
                }
            }
        });

        return identical;
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
