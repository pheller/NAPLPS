// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

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
    private static readonly float _refCapHeight;

    // NAPLPS Spec-defined width classes (0-9) for ASCII 0x20-0x7E
    // From docs/NAPLPS.ASC lines 2227-2245
    // Class 9 = widest (full character field width), Class 0 = narrowest
    // Goal: "make the interfont gap between all characters identical"
    private static readonly byte[] _asciiWidthClass = new byte[95]
    {
        // 0x20-0x2F: space ! " # $ % & ' ( ) * + , - . /
        9, 0, 4, 6, 9, 9, 9, 0, 1, 1, 9, 9, 3, 5, 0, 9,
        // 0x30-0x3F: 0 1 2 3 4 5 6 7 8 9 : ; < = > ?
        5, 1, 5, 5, 5, 5, 5, 5, 5, 5, 0, 3, 5, 8, 5, 8,
        // 0x40-0x4F: @ A B C D E F G H I J K L M N O
        9, 5, 5, 5, 5, 5, 5, 8, 5, 2, 5, 5, 5, 9, 5, 9,
        // 0x50-0x5F: P Q R S T U V W X Y Z [ \ ] ^ _
        5, 6, 5, 5, 9, 5, 9, 9, 9, 9, 9, 4, 9, 4, 2, 9,
        // 0x60-0x6F: ` a b c d e f g h i j k l m n o
        1, 5, 5, 5, 5, 5, 5, 5, 5, 0, 4, 5, 0, 9, 5, 5,
        // 0x70-0x7E: p q r s t u v w x y z { | } ~
        5, 5, 5, 5, 2, 5, 9, 9, 9, 5, 5, 5, 0, 5, 9
    };

    // Displacement table row 8 from NAPLPS spec (for text sizes < 12/256)
    // Maps width class (0-9) to displacement value (2-8)
    private static readonly int[] _displacementRow8 = { 2, 3, 4, 4, 5, 6, 7, 6, 7, 8 };

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

        var refCapBounds = TextMeasurer.MeasureBounds("H", new TextOptions(refFont));
        _refCapHeight = refCapBounds.Height;
    }

    /// <summary>
    /// Gets the width ratio of a character relative to full cell width.
    /// Uses NAPLPS spec-defined width classes and displacement table.
    /// </summary>
    public static float GetCharWidthRatio(char c)
    {
        if (c < 0x20 || c > 0x7E)
        {
            return 1.0f; // Full width for unknown characters
        }

        int widthClass = _asciiWidthClass[c - 0x20];
        // Convert displacement value (2-8) to ratio (0.25-1.0)
        return _displacementRow8[widthClass] / 8f;
    }

    /// <summary>
    /// Gets a horizontal stretch boost factor for thin characters.
    /// These characters have lots of built-in whitespace in the font and need
    /// extra stretching to fill their cells properly.
    /// </summary>
    private static float GetThinCharacterBoost(char c)
    {
        return c switch
        {
            'i' or 'l' or ':' or '.' or '\'' => 4.0f,   // Very thin - need 2x stretch
            '1' => 2.0f,  // Moderately thin
            't' or 'I' or '!' or '.' or ',' or ';' => 1.8f,  // Thin punctuation
            '(' or ')' or '[' or ']' or '|' => 1.6f,  // Brackets/pipe
            'j' => 1.4f,
            'f' => 1.2f,
            'w' or 'W' or 'X' or 'Y' or 'G' or 'm' or 'x' or 'M' or 'T' or '@' or '/' or '&' => 0.8f, // Too big
            'v' or 'V' => 0.6f, // Really big!
            _ => 1.0f,  // Normal characters - no boost
        };
    }

    private static void GetCharXBackoff(ref float offsetX, char character)
    {
        switch (character)
        {
            case 't':
            case '1':
            case ',':
            {
                offsetX -= 10;
            }
            break;

            case 'I':
            {
                offsetX -= 8;
            }
            break;

            case 'R':
            {
                offsetX -= 4;
            }
            break;
        }
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

        var (fgColor, bgColor) = GetISColorFromState(state);

        // Reverse video: swap foreground and background colors
        if (state.IsReverseVideo)
        {
            (fgColor, bgColor) = (bgColor, fgColor);
        }

        // Use a fixed font size, then STRETCH it to fit the cell.
        // This mimics how old NAPLPS renderers worked — they'd blit a bitmap
        // font and stretch it to fit the character field dimensions.
        float fontSize = 100f;
        var font = _fontFamily.CreateFont(fontSize, FontStyle.Regular);

        // Calculate scale factors to stretch the glyph to fit the cell
        // Full horizontal fill, slight vertical margin for authentic spacing
        float targetW = cellW;
        float targetH = cellH * 0.90f;

        float baseScaleX = targetW / _refCharWidth;
        float scaleY = targetH / _refLineHeight;

        // For thin characters, apply a horizontal stretch boost to fill the cell better
        // These characters have lots of built-in whitespace in the font design
        float boostFactor = GetThinCharacterBoost(_command.AsciiCharacter);
        float scaleX = baseScaleX * boostFactor;

        // Adjust X position to center the boosted glyph in its cell
        float baseGlyphWidth = _refCharWidth * baseScaleX;
        float boostedGlyphWidth = _refCharWidth * scaleX;
        float extraWidth = boostedGlyphWidth - baseGlyphWidth;
        float adjustedCellTopX = cellTopX - extraWidth / 2f;

        // Scaled dimensions
        float scaledLineHeight = _refLineHeight * scaleY;
        float scaledTopOffset = _refTopOffset * scaleY;

        // Center on cap height so text aligns visually with adjacent elements.
        // Full _refLineHeight includes descender space which shifts caps upward.
        float scaledCapHeight = _refCapHeight * scaleY;
        float vertPad = MathF.Min((cellH - scaledCapHeight) / 2f, cellH - scaledLineHeight);

        // Position for the glyph's top to land at cellTopY + vertPad
        // But we need to account for the transform scaling around origin
        float textOriginY = (cellTopY + vertPad - scaledTopOffset) / scaleY;
        float textOriginX = adjustedCellTopX / scaleX;

        GetCharXBackoff(ref textOriginX, _command.AsciiCharacter);

        // Create transform that scales from origin (0,0)
        var transform = Matrix3x2.CreateScale(scaleX, scaleY);

        var drawingOptions = new DrawingOptions
        {
            Transform = transform
        };

        var charText = _command.AsciiCharacter.ToString();

        image.Mutate(ctx =>
        {
            // In color mode 2, fill character field with background color
            // (modes 0 and 1 only draw foreground pixels, no background fill)
            if (state.ColorMode == 2)
            {
                // Extend width by 1px to avoid gaps between adjacent characters
                var bgRect = new RectangleF(rect.X, rect.Y, rect.Width + 1, rect.Height);

                ctx.Fill(new DrawingOptions(), bgColor, bgRect);
            }

            if (Options.DebugTextDrawing)
            {
                if (state.ColorMode != 2)
                {
                    ctx.Fill(new DrawingOptions(), bgColor, rect);
                }

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

            // Underline: draw line at bottom of character field
            if (state.IsUnderline)
            {
                float underlineY = penPoint.Y - 1; // Just above the pen position (bottom of cell)
                float underlineThickness = MathF.Max(1f, cellH * 0.05f);
                var underlinePen = Pens.Solid(fgColor, underlineThickness);
                ctx.DrawLine(underlinePen, new PointF(cellTopX, underlineY), new PointF(cellTopX + cellW, underlineY));
            }
        });
    }
}
