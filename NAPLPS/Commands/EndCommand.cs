// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// This command terminates the current DEF MACRO, DEFP
/// MACRO, DEFT MACRO, DEF ORCS, or DEF TEXTURE operation.It is also
/// used in the transmission of data in an unprotected field (see 6.2.6).
/// </summary>
public class EndCommand(NaplpsState state, NaplpsOperands operands) : NaplpsCommand(state, ESC, operands)
{
} 