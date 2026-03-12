// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class NonSelectiveResetCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : NaplpsCommand(state, opcode, operands)
{
}