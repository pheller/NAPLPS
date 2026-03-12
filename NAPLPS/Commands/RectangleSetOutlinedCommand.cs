// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class RectangleSetOutlinedCommand : RectangleSetCommand
{
    public RectangleSetOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        ShouldFill = false;
    }
}