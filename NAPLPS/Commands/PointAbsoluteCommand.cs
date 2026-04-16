// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Point Absolute", "Draw a point at an absolute location.", Category = CommandCategory.Geometric, DslKeyword = "pointAbs")]
public class PointAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : PointCommand(false, state, opcode, operands)
{
}