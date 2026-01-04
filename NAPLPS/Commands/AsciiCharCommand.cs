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
        pen.X += state.CharSize.X;
        state.Pen = pen;
    }

    public override string ToString()
    {
        return $"ASCII({AsciiCharacter})";
    }
}
