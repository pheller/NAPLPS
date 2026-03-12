// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class LineSetRelativeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : LineCommand(true, true, state, opcode, operands)
{
}