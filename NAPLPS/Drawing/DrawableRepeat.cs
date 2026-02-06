// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Drawing;

/// <summary>
/// Handles rendering of the Repeat (0x86) and RepeatToEOL (0x87) control commands.
/// These commands repeat the last displayed character N times.
/// </summary>
public class DrawableRepeat : Drawable, IDrawable
{
    private readonly ControlCommand _command;
    private readonly bool _isRepeatToEOL;

    public DrawableRepeat(ControlCommand command) : base(command)
    {
        _command = command;
        _isRepeatToEOL = command.Command == NaplpsControlCommands.RepeatToEOL;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        // This is handled specially by DrawContext which tracks the last character
        // and calls RenderRepeat directly
    }

    /// <summary>
    /// Gets the repeat count from the command operands.
    /// For Repeat: count is in operands[0], values 0xC0-0xFF have 0x40 subtracted.
    /// For RepeatToEOL: count is calculated based on remaining line width.
    /// </summary>
    public int GetRepeatCount(NaplpsState state)
    {
        if (_isRepeatToEOL)
        {
            // Calculate characters to end of field
            var fieldEndX = state.Field.Origin.X + state.Field.Dimensions.X;
            return Math.Max(0, (int)((fieldEndX - state.Pen.X) / state.CharSize.X));
        }

        if (_command.Operands.Count > 0)
        {
            var countByte = _command.Operands[0];
            // Values in 0xC0-0xFF range (numerical data) have 0x40 subtracted
            return countByte >= 0xC0 ? countByte - 0x40 : countByte;
        }

        return 0;
    }

    public bool IsRepeatToEOL => _isRepeatToEOL;
}
