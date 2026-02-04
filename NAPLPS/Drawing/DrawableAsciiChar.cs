// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using FontFamily = SixLabors.Fonts.FontFamily;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableAsciiChar : Drawable, IDrawable
{
    private readonly AsciiCharCommand _command;
    private static readonly FontCollection _fontCollection = new();
    private static readonly FontFamily _fontFamily;

    // Pre-computed reference measurements at size 100
    private static readonly float _refLineHeight;
    private static readonly float _refTopOffset;
    private static readonly float _refCharWidth;

    // Width ratios for proportional spacing - conservative values to avoid squishing
    // Range: 0.6 (narrow) to 1.0 (wide) - don't go below 0.6 or glyphs get too thin
    private static readonly Dictionary<char, float> _charWidthRatios = new()
    {
        // Punctuation - narrow but not too narrow
        [' '] = 0.70f,
        ['!'] = 0.60f,
        ['"'] = 0.75f,
        ['#'] = 0.90f,
        ['$'] = 0.85f,
        ['%'] = 1.00f,
        ['&'] = 0.90f,
        ['\''] = 0.60f,
        ['('] = 0.65f,
        [')'] = 0.65f,
        ['*'] = 0.75f,
        ['+'] = 0.80f,
        [','] = 0.60f,
        ['-'] = 0.70f,
        ['.'] = 0.60f,
        ['/'] = 0.70f,

        // Numbers - uniform width
        ['0'] = 0.85f,
        ['1'] = 0.70f,
        ['2'] = 0.85f,
        ['3'] = 0.85f,
        ['4'] = 0.85f,
        ['5'] = 0.85f,
        ['6'] = 0.85f,
        ['7'] = 0.85f,
        ['8'] = 0.85f,
        ['9'] = 0.85f,

        // More punctuation
        [':'] = 0.60f,
        [';'] = 0.60f,
        ['<'] = 0.75f,
        ['='] = 0.80f,
        ['>'] = 0.75f,
        ['?'] = 0.75f,
        ['@'] = 1.00f,

        // Uppercase letters
        ['A'] = 0.90f,
        ['B'] = 0.85f,
        ['C'] = 0.85f,
        ['D'] = 0.90f,
        ['E'] = 0.80f,
        ['F'] = 0.80f,
        ['G'] = 0.90f,
        ['H'] = 0.90f,
        ['I'] = 0.60f,
        ['J'] = 0.75f,
        ['K'] = 0.85f,
        ['L'] = 0.80f,
        ['M'] = 1.00f,
        ['N'] = 0.90f,
        ['O'] = 0.95f,
        ['P'] = 0.85f,
        ['Q'] = 0.95f,
        ['R'] = 0.85f,
        ['S'] = 0.85f,
        ['T'] = 0.85f,
        ['U'] = 0.90f,
        ['V'] = 0.90f,
        ['W'] = 1.00f,
        ['X'] = 0.85f,
        ['Y'] = 0.85f,
        ['Z'] = 0.85f,

        // Brackets and symbols
        ['['] = 0.65f,
        ['\\'] = 0.70f,
        [']'] = 0.65f,
        ['^'] = 0.75f,
        ['_'] = 0.85f,
        ['`'] = 0.60f,

        // Lowercase letters
        ['a'] = 0.80f,
        ['b'] = 0.80f,
        ['c'] = 0.75f,
        ['d'] = 0.80f,
        ['e'] = 0.80f,
        ['f'] = 0.65f,
        ['g'] = 0.80f,
        ['h'] = 0.80f,
        ['i'] = 0.60f,
        ['j'] = 0.60f,
        ['k'] = 0.75f,
        ['l'] = 0.60f,
        ['m'] = 1.00f,
        ['n'] = 0.80f,
        ['o'] = 0.80f,
        ['p'] = 0.80f,
        ['q'] = 0.80f,
        ['r'] = 0.70f,
        ['s'] = 0.75f,
        ['t'] = 0.65f,
        ['u'] = 0.80f,
        ['v'] = 0.75f,
        ['w'] = 1.00f,
        ['x'] = 0.75f,
        ['y'] = 0.75f,
        ['z'] = 0.75f,

        // More symbols
        ['{'] = 0.70f,
        ['|'] = 0.60f,
        ['}'] = 0.70f,
        ['~'] = 0.80f,
    };

    static DrawableAsciiChar()
    {
        var assembly = typeof(DrawableAsciiChar).Assembly;

        // Use PRM5X10 as the base font — it's the "standard" NAPLPS font
        using var stream = assembly.GetManifestResourceStream("NAPLPS.Fonts.PRM5X10.TTF");

        if (stream == null)
        {
            throw new InvalidOperationException("Could not load embedded font resource.");
        }

        _fontFamily = _fontCollection.Add(stream);

        // Measure reference bounds at size 100
        var refFont = _fontFamily.CreateFont(100f, FontStyle.Regular);
        var refBounds = TextMeasurer.MeasureBounds("Mg", new TextOptions(refFont));
        var refWidthBounds = TextMeasurer.MeasureBounds("M", new TextOptions(refFont));

        _refLineHeight = refBounds.Height;
        _refTopOffset = refBounds.Top;
        _refCharWidth = refWidthBounds.Width;
    }

    /// <summary>
    /// Gets the width ratio of a character relative to full cell width.
    /// Used for proportional text spacing.
    /// </summary>
    public static float GetCharWidthRatio(char c)
    {
        if (_charWidthRatios.TryGetValue(c, out float ratio))
        {
            return ratio;
        }
        // Fallback to average width for unknown characters
        return 0.70f;
    }

    public DrawableAsciiChar(AsciiCharCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        // Convert the pen (normalized NAPLPS coords) to screen pixel coordinates.
        var penPoint = ConvertNormalizedToPoint(size, state.Pen.X, state.Pen.Y);

        // Convert character cell size (normalized) to screen pixels
        var (charSizeX, charSizeY) = ConvertNormalizedToScreenScale(size, state.CharSize.X, state.CharSize.Y);

        // Get proportional width for this character
        float widthRatio = GetCharWidthRatio(_command.AsciiCharacter);

        // Apply proportional width to cell
        float cellW = MathF.Max(1f, MathF.Abs(charSizeX) * widthRatio);
        float cellH = MathF.Max(1f, MathF.Abs(charSizeY));

        // Cell top-left in screen coords (Y-down)
        float cellTopX = penPoint.X;
        float cellTopY = penPoint.Y - cellH;

        var rect = new RectangleF(cellTopX, cellTopY, cellW, cellH);

        var (fgColor, bgColor) = GetISColorFromState();

        // Use a fixed font size, then STRETCH it to fit the cell.
        // This mimics how old NAPLPS renderers worked — they'd blit a bitmap
        // font and stretch it to fit the character field dimensions.
        float fontSize = 100f;
        var font = _fontFamily.CreateFont(fontSize, FontStyle.Regular);

        // Calculate scale factors to stretch the glyph to fit the cell
        // Leave a small margin (90% fill) for authentic spacing
        float targetW = cellW * 0.85f;
        float targetH = cellH * 0.90f;

        float scaleX = targetW / _refCharWidth;
        float scaleY = targetH / _refLineHeight;

        // Scaled dimensions
        float scaledLineHeight = _refLineHeight * scaleY;
        float scaledTopOffset = _refTopOffset * scaleY;

        // Center vertically
        float vertPad = (cellH - scaledLineHeight) / 2f;

        // Position for the glyph's top to land at cellTopY + vertPad
        // But we need to account for the transform scaling around origin
        float textOriginY = (cellTopY + vertPad - scaledTopOffset) / scaleY;
        float textOriginX = cellTopX / scaleX;

        // Create transform that scales from origin (0,0)
        var transform = Matrix3x2.CreateScale(scaleX, scaleY);

        var drawingOptions = new DrawingOptions
        {
            Transform = transform
        };

        var charText = _command.AsciiCharacter.ToString();

        image.Mutate(ctx =>
        {
            if (Options.DebugTextDrawing)
            {
                ctx.Fill(new DrawingOptions(), bgColor, rect);

                var debugStrokePen = Pens.Solid(fgColor, 1f);
                var debugDashedPen = Pens.Dash(fgColor, 1f);

                // Crosshair at pen origin (bottom-left of the box)
                ctx.DrawLine(debugStrokePen, new PointF(cellTopX - 4, penPoint.Y), new PointF(cellTopX + 4, penPoint.Y));
                ctx.DrawLine(debugStrokePen, new PointF(cellTopX, penPoint.Y - 4), new PointF(cellTopX, penPoint.Y + 4));
                ctx.Draw(debugStrokePen, rect);

                // ASCII code label
                var labelFontSize = MathF.Max(8f, MathF.Min(14f, cellW));
                var labelFont = _fontFamily.CreateFont(labelFontSize, FontStyle.Regular);
                var labelText = $"{(int)_command.AsciiCharacter}";
                ctx.DrawText(labelText, labelFont, fgColor, new PointF(cellTopX + 2, cellTopY + 2));

                // Baseline reference line
                float debugBaseline = cellTopY + vertPad + scaledLineHeight;
                ctx.DrawLine(debugDashedPen, new PointF(cellTopX, debugBaseline), new PointF(cellTopX + cellW, debugBaseline));
            }

            // Draw with stretch transform
            ctx.DrawText(drawingOptions, charText, font, fgColor, new PointF(textOriginX, textOriginY));
        });
    }
}
