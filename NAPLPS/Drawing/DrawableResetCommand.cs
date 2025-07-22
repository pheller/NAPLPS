// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace NAPLPS.Drawing;

public class DrawableResetCommand : IDrawable
{
    private readonly ResetCommand _command;

    public DrawableResetCommand(ResetCommand command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackground].ToColor();

        if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenBlack)
        {
            image.Mutate(x => x.Fill(ISColor.Black));
        }
        else if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenDrawing)
        {
            image.Mutate(x => x.Fill(fgcolor.ToISColor()));
        }
        else if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenBorderDrawing)
        {
            image.Mutate(x => x.Fill(fgcolor.ToISColor()));
        }
        else if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenDrawingBorderBlack)
        {
            image.Mutate(x => x.Fill(fgcolor.ToISColor()));
        }
        else if (_command.ColorScreenBorder == ResetCommand.ScreenBorderReset.ScreenBorderBlack)
        {
            image.Mutate(x => x.Fill(ISColor.Black));
        }
    }
}
