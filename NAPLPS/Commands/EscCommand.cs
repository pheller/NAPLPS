// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// This character (0x1B) is used for code extension (see 4.3.2 and 4.3.3).
/// </summary>
public class EscCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : NaplpsCommand(state, opcode, operands)
{
}
