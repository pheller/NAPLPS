// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class LineSetAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : LineCommand(true, false, state, opcode, operands)
{
}