// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PointAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : PointCommand(false, state, opcode, operands)
{
}