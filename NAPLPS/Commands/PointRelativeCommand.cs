// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Point Relative", "Draw a point at a relative offset from the pen.", Category = CommandCategory.Geometric, DslKeyword = "pointRel")]
public class PointRelativeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : PointCommand(true, state, opcode, operands)
{
}