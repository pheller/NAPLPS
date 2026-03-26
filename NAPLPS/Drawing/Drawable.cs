// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using Brush = SixLabors.ImageSharp.Drawing.Processing.Brush;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using SolidBrush = SixLabors.ImageSharp.Drawing.Processing.SolidBrush;
using SixLabors.ImageSharp.Processing;
using TexturePatterns = NAPLPS.NaplpsTexture.TexturePatterns;

namespace NAPLPS.Drawing;

/// <summary>Used on every drawable shape or command.</summary>
public class Drawable
{
    public static class Options
    {
        public static bool DebugTextDrawing { get; set; } = false;

        /// <summary>
        /// When true, uses bitmap font rendering (VGA 8x16 style, nearest-neighbor scaling)
        /// for pixel-accurate PP3-matching output. When false, uses TrueType font rendering
        /// with system font fallback for high-resolution output.
        /// </summary>
        public static bool UseBitmapFont { get; set; } = false;
    }

    /// <summary>
    /// When set, color resolution uses this palette instead of the per-command state's ColorMap.
    /// This enables palette animation (blink, palette cycling) — modifications to LivePalette
    /// are immediately visible on re-render without changing historical state snapshots.
    /// Rendering is single-threaded, so a static property is safe here.
    /// </summary>
    public static Dictionary<byte, NaplpsColor>? LivePalette { get; set; }

    private readonly NaplpsCommand _baseCommand;
    private readonly NaplpsState _state;

    public Drawable(NaplpsCommand baseCommand)
    {
        _baseCommand = baseCommand;
        _state = _baseCommand.State ?? new();
    }

    public System.Drawing.Point GetScaledLogicalPel(Size size)
    {
        var drawingCommand = (GeometricDrawingCommandBase)_baseCommand;
        var logicalPel = drawingCommand.LogicalPel;
        var (pelX, pelY) = ConvertNormalizedToScreenScale(size, logicalPel.X, logicalPel.Y);

        return new System.Drawing.Point(Math.Max(1, Math.Abs(pelX)), Math.Max(1, Math.Abs(pelY)));
    }

    public float GetPenWidth(Size size)
    {
        var scaledLogicalPel = GetScaledLogicalPel(size);

        return Math.Max(scaledLogicalPel.X, scaledLogicalPel.Y);
    }

    /// <summary>
    /// Gets the pen width using float precision (avoids integer truncation artifacts
    /// for outlines that need to align precisely with subsequent fills).
    /// </summary>
    public float GetPenWidthF(Size size)
    {
        var drawingCommand = (GeometricDrawingCommandBase)_baseCommand;
        var logicalPel = drawingCommand.LogicalPel;
        float pelX = MathF.Abs(logicalPel.X * size.Width);
        float pelY = MathF.Abs(logicalPel.Y / 0.80f * size.Height);
        return MathF.Max(MathF.Max(pelX, pelY), 1f);
    }

    internal (Brush, Pen) GetBrushAndPenFromFillableCommand(Size size)
    {
        var fillableCommand = (FillableGeometricDrawingCommandBase)_baseCommand;

        var (fgColor, bgColor) = fillableCommand.GetColors(_state);

        var brush = GetFillBrush(size, fgColor, bgColor);

        var penWidth = GetPenWidth(size);
        Pen pen;

        if (fillableCommand.ShouldFill && fillableCommand.Texture.ShouldHighlight)
        {
            // ANSI X3.110: Highlighted filled shapes are outlined with SOLID line texture
            // (ignoring current line texture), using nominal black in modes 0/1,
            // or background color in mode 2.
            var highlightColor = _state.ColorMode == 2 ? bgColor : Color.Black;
            pen = Pens.Solid(highlightColor.ToISColor(), penWidth);
        }
        else
        {
            var penColor = fillableCommand.ShouldFill ? bgColor : fgColor;
            pen = GetTexturedPen(penColor.ToISColor(), penWidth);
        }

        return (brush, pen);
    }

    internal Pen GetTexturedPen(SixLabors.ImageSharp.Color color, float penWidth)
    {
        var fillableCommand = (GeometricDrawingCommandBase)_baseCommand;
        var lineTexture = fillableCommand.Texture.LineTexture;

        // ANSI X3.110 line texture patterns are defined in terms of logical pel size:
        // - Dot: 1 pel on, 1 pel off
        // - Dash: 3 pels on, 1 pel off
        // - Dot-Dash: 1 pel on, 1 pel off, 3 pels on, 1 pel off
        // PatternPen multiplies each entry by the pen width (which is the pel size).
        switch (lineTexture)
        {
            case NaplpsTexture.LineTextures.Dotted:
            {
                return new PatternPen(color, penWidth, new float[] { 1f, 1f });
            }

            case NaplpsTexture.LineTextures.Dashed:
            {
                return new PatternPen(color, penWidth, new float[] { 3f, 1f });
            }

            case NaplpsTexture.LineTextures.DottedDashed:
            {
                return new PatternPen(color, penWidth, new float[] { 1f, 1f, 3f, 1f });
            }

            default:
            {
                return Pens.Solid(color, penWidth);
            }
        }
    }

    internal Brush GetFillBrush(Size size, Color fgColor, Color bgColor)
    {
        var fillableCommand = (GeometricDrawingCommandBase)_baseCommand;
        var texturePattern = fillableCommand.Texture.TexturePattern;

        var fgColorImageSharp = fgColor.ToISColor();
        var bgColorImageSharp = bgColor.ToISColor();

        if (fillableCommand.ColorMode != 2)
        {
            bgColorImageSharp = ISColor.Transparent;
        }

        var scaledLogicalPel = GetScaledLogicalPel(size);

        switch (texturePattern)
        {
            case TexturePatterns.MaskA:
            case TexturePatterns.VerticalHatching:
            {
                var pattern = new bool[1, scaledLogicalPel.X * 2];

                for (var i = 0; i < pattern.Length; ++i)
                {
                    pattern[0, i] = i >= pattern.Length / 2;
                }

                return new PatternBrush(fgColorImageSharp, bgColorImageSharp, pattern);
            }

            case TexturePatterns.MaskB:
            case TexturePatterns.HorizontalHatching:
            {
                // ANSI X3.110: horizontal lines 1 logical pel HEIGHT high, spaced 2x pelH apart
                var pelY = Math.Max(1, scaledLogicalPel.Y);
                var pattern = new bool[pelY * 2, 1];

                for (var i = 0; i < pattern.Length; ++i)
                {
                    pattern[i, 0] = i >= pelY;
                }

                return new PatternBrush(fgColorImageSharp, bgColorImageSharp, pattern);
            }

            case TexturePatterns.MaskC:
            case TexturePatterns.CrossHatching:
            {
                // ANSI X3.110: combination of vertical (pelW wide) and horizontal (pelH high)
                var pelX = Math.Max(1, scaledLogicalPel.X);
                var pelY = Math.Max(1, scaledLogicalPel.Y);
                var width = pelX * 2;
                var height = pelY * 2;
                var pattern = new bool[height, width];

                for (var y = 0; y < height; ++y)
                {
                    for (var x = 0; x < width; ++x)
                    {
                        pattern[y, x] = y >= pelY || x < pelX;
                    }
                }

                return new PatternBrush(fgColorImageSharp, bgColorImageSharp, pattern);
            }

            default:
            {
                return Brushes.Solid(fgColorImageSharp);
            }
        }
    }

    internal (NaplpsColor, NaplpsColor) GetNaplpsColorFromState(NaplpsState? state)
    {
        if (state == null)
        {
            state = _state;
        }

        // Use LivePalette for color lookups when available (enables palette animation).
        // The color index comes from historical state, but the actual color value
        // comes from the live palette — this is how real NAPLPS terminals work (CLUT swap).
        var palette = LivePalette ?? state.ColorMap;
        var fgColor = state.ColorMode == 0 ? state.Foreground : palette[state.ColorMapForeground];
        var bgColor = state.ColorMode == 0 ? state.Background : palette[state.ColorMapBackground];

        return (fgColor, bgColor);
    }

    internal (Color, Color) GetColorFromState(NaplpsState? state)
    {
        var (fgColor, bgColor) = GetNaplpsColorFromState(state);

        return (fgColor.ToColor(), bgColor.ToColor());
    }

    internal (ISColor, ISColor) GetISColorFromState(NaplpsState? state)
    {
        var (fgColor, bgColor) = GetColorFromState(state);

        return (fgColor.ToISColor(), bgColor.ToISColor());
    }

    internal (SolidBrush, SolidPen) GetBrushAndPenFromState(NaplpsState? state)
    {
        var (fgColor, bgColor) = GetISColorFromState(state);

        return (Brushes.Solid(bgColor), Pens.Solid(fgColor, state.LogicalPel.X == 0 ? 1 : state.LogicalPel.X));
    }

    /// <summary>
    /// Draws a shape outline using rectangular logical pel sweep.
    /// Sweeps the pel along each edge of the given polygon points.
    /// </summary>
    internal void DrawOutlineWithPelSweep(IImageProcessingContext ctx, PointF[] points, ISColor color, Size size, bool closePath = true)
    {
        var scaledPel = GetScaledLogicalPel(size);
        float pelW = scaledPel.X;
        float pelH = scaledPel.Y;

        int edgeCount = closePath ? points.Length : points.Length - 1;

        for (int i = 0; i < edgeCount; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Length];
            var hull = DrawableLine.ConvexHullOfSweptPel(p1, p2, pelW, pelH);
            ctx.FillPolygon(color, hull);
        }
    }

    /// <summary>
    /// Gets the outline color for a fillable command per NAPLPS spec.
    /// Non-filled: foreground color. Filled+highlight: nominal black (modes 0/1) or background (mode 2).
    /// </summary>
    internal ISColor GetOutlineColor()
    {
        var fillableCommand = (FillableGeometricDrawingCommandBase)_baseCommand;
        var (fgColor, bgColor) = fillableCommand.GetColors(_state);

        if (fillableCommand.ShouldFill && fillableCommand.Texture.ShouldHighlight)
        {
            return (_state.ColorMode == 2 ? bgColor : Color.Black).ToISColor();
        }

        return (fillableCommand.ShouldFill ? bgColor : fgColor).ToISColor();
    }
}