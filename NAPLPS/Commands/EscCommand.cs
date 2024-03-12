// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// This character (0x1B) is used for code extension (see 4.3.2 and 4.3.3).
/// </summary>
public class EscCommand(NaplpsState state, NaplpsOperands operands) : NaplpsCommand(state, ESC, operands)
{
}