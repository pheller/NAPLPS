// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Line Set Absolute", "Draw a connected polyline through a sequence of absolute points.", Category = CommandCategory.Geometric, DslKeyword = "lineSetAbs")]
public class LineSetAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : LineCommand(true, false, state, opcode, operands)
{
}