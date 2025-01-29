// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class NonSelectiveResetCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : NaplpsCommand(state, opcode, operands)
{
}