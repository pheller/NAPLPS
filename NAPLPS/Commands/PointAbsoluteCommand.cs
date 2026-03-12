// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PointAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : PointCommand(false, state, opcode, operands)
{
}