// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Point Set Absolute", "Move the active position to an absolute location without drawing.", Category = CommandCategory.Geometric, DslKeyword = "moveAbs")]
public class PointSetAbsoluteCommand : PointCommand
{
    public PointSetAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, false, state, opcode, operands)
    {
    }
}