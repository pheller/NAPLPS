// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// </summary>
public class C1Command(NaplpsState state, byte opcode, NaplpsOperands operands) : EscCommand(state, opcode, operands)
{
}