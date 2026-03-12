// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PointRelativeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : PointCommand(true, state, opcode, operands)
{
}