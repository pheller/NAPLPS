// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class RectangleOutlinedCommand : RectangleCommand
{
    public RectangleOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        ShouldFill = false;
    }
}