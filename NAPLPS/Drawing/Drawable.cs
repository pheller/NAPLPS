// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;

using Brush = SixLabors.ImageSharp.Drawing.Processing.Brush;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using SolidBrush = SixLabors.ImageSharp.Drawing.Processing.SolidBrush;
using TexturePatterns = NAPLPS.NaplpsTexture.TexturePatterns;

namespace NAPLPS.Drawing;

/// <summary>Used on every drawable shape or command.</summary>
public class Drawable
{
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

        return new System.Drawing.Point(
            (int)(Math.Abs(logicalPel.X * size.Width)) + 1,
            (int)(Math.Abs(logicalPel.Y * size.Height)) + 1
        );
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
                return Pens.Dot(color, penWidth);

            case NaplpsTexture.LineTextures.DottedDashed:
                return Pens.DashDot(color, penWidth);

            case NaplpsTexture.LineTextures.Dashed:
                return Pens.Dash(color, penWidth);

            default:
                return Pens.Solid(color, penWidth);
        }
    }

    internal Brush GetFillBrush(Size size, Color fgColor, Color bgColor)
    {
        var fillableCommand = (GeometricDrawingCommandBase)_baseCommand;
        var texturePattern = fillableCommand.Texture.TexturePattern;

        var fgColorImageSharp = fgColor.ToISColor();
        var bgColorImageSharp = bgColor.ToISColor();

        var scaledLogicalPel = GetScaledLogicalPel(size);

        switch (texturePattern)
        {
            case TexturePatterns.VerticalHatching:
            {
                var pattern = new bool[1, scaledLogicalPel.X * 2];

                for (var i = 0; i < pattern.Length; ++i)
                {
                    pattern[0, i] = i >= pattern.Length / 2;
                }

                return new PatternBrush(
                    fgColorImageSharp,
                    bgColorImageSharp,
                    pattern
                );
            }

            case TexturePatterns.HorizontalHatching:
            {
                var pattern = new bool[scaledLogicalPel.X * 2, 1];

                for (var i = 0; i < pattern.Length; ++i)
                {
                    pattern[i, 0] = i >= pattern.Length / 2;
                }

                return new PatternBrush(
                    fgColorImageSharp,
                    bgColorImageSharp,
                    pattern
                );
            }

            case TexturePatterns.CrossHatching:
            {
                return new PatternBrush(
                    fgColorImageSharp,
                    bgColorImageSharp,
                    new bool[,] {
                        {false, true},
                        {false, false}
                    }
                );
            }

            default:
            {
                return Brushes.Solid(fgColorImageSharp);
            }
        }
    }

    internal (SolidBrush, SolidPen) GetBrushAndPenFromState()
    {
        var bgcolor = _state.ColorMode == 2 ? _state.Foreground.ToColor() : _state.ColorMap[_state.ColorMapForeground].ToColor();
        var fgcolor = _state.ColorMode == 2 ? _state.Background.ToColor() : _state.ColorMap[_state.ColorMapBackground].ToColor();

        return (
            Brushes.Solid(bgcolor.ToISColor()),
            Pens.Solid(fgcolor.ToISColor(), _state.LogicalPel.X == 0 ? 1 : _state.LogicalPel.X)
        );
    }
}
