// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// This character 0x18 is used to terminate processing
/// of all currently executing macros.Execution is resumed at the next
/// presentation layer character following the terminated macro call.The effect
/// of CAN is immediate, ie, it is not put at the end of any existing queue of
/// unprocessed presentation layer code. The operation of the CAN character is
/// not guaranteed unless it is guaranteed to be delivered by the lower layers.
/// </summary>
public class CancelCommand(NaplpsState state, NaplpsOperands operands) : NaplpsCommand(state, CANCEL, operands)
{
}