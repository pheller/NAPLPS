// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Line Absolute", "Draw a line from the pen to an absolute location.", Category = CommandCategory.Geometric, DslKeyword = "lineAbs")]
public class LineAbsoluteCommand : LineCommand
{
    public LineAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, false, state, opcode, operands)
    {
    }
}