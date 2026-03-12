// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Drawing;

namespace NAPLPS.Commands;

public class AsciiCharCommand : NaplpsCommand
{
    public char AsciiCharacter { get; }

    public AsciiCharCommand(char asciiCharacter, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        AsciiCharacter = asciiCharacter;

        MovePen(state);
    }

    private void MovePen(NaplpsState state)
    {
        var pen = state.Pen;

        float spacingMultiplier = state.TextSpacing switch
        {
            TextSpacing.One => 1.0f,
            TextSpacing.FiveQuarters => 1.25f,
            TextSpacing.ThreeHalves => 1.5f,
            TextSpacing.Proportional => 1.0f,
            _ => 1.0f
        };

        // Get proportional width ratio for this character
        float widthRatio = DrawableAsciiChar.GetCharWidthRatio(AsciiCharacter);

        switch (state.TextPath)
        {
            case TextPath.Right:
            pen.X += state.CharSize.X * widthRatio * spacingMultiplier;
            break;
            case TextPath.Left:
            pen.X -= state.CharSize.X * widthRatio * spacingMultiplier;
            break;
            case TextPath.Up:
            pen.Y += state.CharSize.Y * spacingMultiplier;
            break;
            case TextPath.Down:
            pen.Y -= state.CharSize.Y * spacingMultiplier;
            break;
        }

        state.Pen = pen;
    }

    public override string ToString()
    {
        return $"ASCII({AsciiCharacter})";
    }
}
