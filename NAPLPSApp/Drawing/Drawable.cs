// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;

using SixLabors.ImageSharp.Drawing.Processing;

using Color = SixLabors.ImageSharp.Color;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using SolidBrush = SixLabors.ImageSharp.Drawing.Processing.SolidBrush;

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
