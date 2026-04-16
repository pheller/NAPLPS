// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Line Set Relative", "Draw a connected polyline through a sequence of relative points.", Category = CommandCategory.Geometric, DslKeyword = "lineSetRel")]
public class LineSetRelativeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : LineCommand(true, true, state, opcode, operands)
{
}