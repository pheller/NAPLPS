// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using Brush = SixLabors.ImageSharp.Drawing.Processing.Brush;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using SolidBrush = SixLabors.ImageSharp.Drawing.Processing.SolidBrush;
using TexturePatterns = NAPLPS.NaplpsTexture.TexturePatterns;

namespace NAPLPS.Drawing;

/// <summary>Used on every drawable shape or command.</summary>
public class Drawable
{
    public static class Options
    {
        public static bool DebugTextDrawing { get; set; } = false;
    }

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

    internal (Brush, Pen) GetBrushAndPenFromFillableCommand(Size size)
    {
        var fillableCommand = (FillableGeometricDrawingCommandBase)_baseCommand;

        var (fgColor, bgColor) = fillableCommand.GetColors(_state);

        var brush = GetFillBrush(size, fgColor, bgColor);

        var penColor = fillableCommand.ShouldFill ? bgColor : fgColor;
        var penWidth = GetPenWidth(size);
        var pen = GetTexturedPen(penColor.ToISColor(), penWidth);

        return (brush, pen);
    }

    internal Pen GetTexturedPen(SixLabors.ImageSharp.Color color, float penWidth)
    {
        var fillableCommand = (GeometricDrawingCommandBase)_baseCommand;
        var lineTexture = fillableCommand.Texture.LineTexture;

        switch (lineTexture)
        {
            case NaplpsTexture.LineTextures.Dotted:
            {
                return Pens.Dot(color, penWidth);
            }

            case NaplpsTexture.LineTextures.DottedDashed:
            {
                return Pens.DashDot(color, penWidth);
            }

            case NaplpsTexture.LineTextures.Dashed:
            {
                return Pens.Dash(color, penWidth);
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
                var pattern = new bool[scaledLogicalPel.X * 2, 1];

                for (var i = 0; i < pattern.Length; ++i)
                {
                    pattern[i, 0] = i >= pattern.Length / 2;
                }

                return new PatternBrush(fgColorImageSharp, bgColorImageSharp, pattern);
            }

            case TexturePatterns.MaskC:
            case TexturePatterns.CrossHatching:
            {
                var pelX = scaledLogicalPel.X;
                var length = pelX * 2;
                var pattern = new bool[length, length];

                for (var y = 0; y < length; ++y)
                {
                    for (var x = 0; x < length; ++x)
                    {
                        pattern[y, x] = y >= pelX || x < pelX;
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

        var fgColor = state.ColorMode == 0 ? state.Foreground : state.ColorMap[state.ColorMapForeground];
        var bgColor = state.ColorMode == 0 ? state.Background : state.ColorMap[state.ColorMapBackground];

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
}