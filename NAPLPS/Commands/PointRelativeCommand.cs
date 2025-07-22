// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PointRelativeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : PointCommand(true, state, opcode, operands)
{
}