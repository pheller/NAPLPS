// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using Color = SixLabors.ImageSharp.Color;
using FontFamily = SixLabors.Fonts.FontFamily;
using PointF = SixLabors.ImageSharp.PointF;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace NAPLPSApp.Drawing;

public class DrawableResetCommand : IDrawable
{
    private readonly ResetCommand _command;

    public DrawableResetCommand(ResetCommand command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
    {
        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackground].ToColor();

        if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenBlack)
        {
            image.Mutate(x => x.Fill(Color.Black));
        }
        else if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenDrawing)
        {
            image.Mutate(x => x.Fill(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A)));
        }
        else if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenBorderDrawing)
        {
            image.Mutate(x => x.Fill(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A)));
        }
        else if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenDrawingBorderBlack)
        {
            image.Mutate(x => x.Fill(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A)));
        }
        else if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenBorderBlack)
        {
            image.Mutate(x => x.Fill(Color.Black));
        }
    }
}
