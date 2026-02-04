// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class AsciiCharCommand : NaplpsCommand
{
    public char AsciiCharacter { get; }

    public AsciiCharCommand(char asciiCharacter, NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        AsciiCharacter = asciiCharacter;

        MovePen(state);
    }

    private static void MovePen(NaplpsState state)
    {
        var pen = state.Pen;

        float multiplier = state.TextSpacing switch
        {
            TextSpacing.One => 1.0f,
            TextSpacing.FiveQuarters => 1.25f,
            TextSpacing.ThreeHalves => 1.5f,
            TextSpacing.Proportional => 1.0f,
            _ => 1.0f
        };

        switch (state.TextPath)
        {
            case TextPath.Right:
                pen.X += state.CharSize.X * multiplier;
                break;
            case TextPath.Left:
                pen.X -= state.CharSize.X * multiplier;
                break;
            case TextPath.Up:
                pen.Y += state.CharSize.Y * multiplier;
                break;
            case TextPath.Down:
                pen.Y -= state.CharSize.Y * multiplier;
                break;
        }

        state.Pen = pen;
    }

    public override string ToString()
    {
        return $"ASCII({AsciiCharacter})";
    }
}
