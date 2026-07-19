// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Point Set Relative", "Move the active position by a relative offset without drawing.", Category = CommandCategory.Geometric, DslKeyword = "moveRel")]
public class PointSetRelativeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : PointCommand(true, false, state, opcode, operands)
{
}