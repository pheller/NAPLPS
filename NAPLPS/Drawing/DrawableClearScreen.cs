// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace NAPLPS.Drawing;

/// <summary>
/// Handles the ClearScreen (0x0C) control command.
/// ANSI X3.110: Clears the display to nominal black in color modes 0/1,
/// or to the background color in color mode 2.
/// </summary>
public class DrawableClearScreen : Drawable, IDrawable
{
    public DrawableClearScreen(ControlCommand command) : base(command)
    {
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        ISColor clearColor;

        if (state.ColorMode == 2)
        {
            var (_, bgColor) = GetISColorFromState(state);
            clearColor = bgColor;
        }
        else
        {
            clearColor = ISColor.Black;
        }

        image.Mutate(x => x.Fill(clearColor));
    }
}
