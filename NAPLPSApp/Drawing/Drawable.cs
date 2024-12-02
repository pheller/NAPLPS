// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;

using SixLabors.ImageSharp.Drawing.Processing;

using Brush = SixLabors.ImageSharp.Drawing.Processing.Brush;
using Color = SixLabors.ImageSharp.Color;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using SolidBrush = SixLabors.ImageSharp.Drawing.Processing.SolidBrush;
using TexturePatterns = NAPLPS.NaplpsTexture.TexturePatterns;
using Size = System.Drawing.Size;

namespace NAPLPSApp.Drawing;

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

    public float GetPenWidth(Size size) {
        var drawingCommand = (GeometricDrawingCommandBase)_baseCommand;
        var logicalPel = drawingCommand.LogicalPel;

        if (logicalPel.X == 0) {
            return 1f;
        }

        return logicalPel.X * size.Width;
    }

    internal (Brush, SolidPen) GetBrushAndPenFromFillableCommand(Size size)
    {
        var fillableCommand = (FillableGeometricDrawingCommandBase)_baseCommand;

        var (fgColor, bgColor) = fillableCommand.GetColors(_state);

        var brush = GetFillBrush(fgColor, bgColor);

        var penColor = fillableCommand.ShouldFill ? bgColor : fgColor;
        var penWidth = GetPenWidth(size);
        var pen = Pens.Solid(Color.FromRgba(penColor.R, penColor.G, penColor.B, penColor.A), penWidth);

        return (brush, pen);
    }

    internal Brush GetFillBrush(System.Drawing.Color fgColor, System.Drawing.Color bgColor)
    {
        var fillableCommand = (GeometricDrawingCommandBase)_baseCommand;
        var texturePattern = fillableCommand.Texture.TexturePattern;

        Color fgColorImageSharp = Color.FromRgba(fgColor.R, fgColor.G, fgColor.B, fgColor.A);
        Color bgColorImageSharp = Color.FromRgba(bgColor.R, bgColor.G, bgColor.B, bgColor.A);

        switch (texturePattern)
        {
            case TexturePatterns.VerticalHatching:
                return new PatternBrush(
                    fgColorImageSharp,
                    bgColorImageSharp,
                    new bool[,] {
                        {false, true}
                    }
                );

            case TexturePatterns.HorizontalHatching:
                return new PatternBrush(
                    fgColorImageSharp,
                    bgColorImageSharp,
                    new bool[,] {
                        {false},
                        {true}
                    }
                );

            case TexturePatterns.CrossHatching:
                return new PatternBrush(
                    fgColorImageSharp,
                    bgColorImageSharp,
                    new bool[,] {
                        {false, true},
                        {true, false}
                    }
                );

            default:
                return Brushes.Solid(fgColorImageSharp);
        }
    }

    internal (SolidBrush, SolidPen) GetBrushAndPenFromState()
    {
        var bgcolor = _state.ColorMode == 2 ? _state.Foreground.ToColor() : _state.ColorMap[_state.ColorMapForeground].ToColor();
        var fgcolor = _state.ColorMode == 2 ? _state.Background.ToColor() : _state.ColorMap[_state.ColorMapBackground].ToColor();

        return (
            Brushes.Solid(Color.FromRgba(bgcolor.R, bgcolor.G, bgcolor.B, bgcolor.A)),
            Pens.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A), _state.LogicalPel.X == 0 ? 1 : _state.LogicalPel.X)
        );
    }
}
