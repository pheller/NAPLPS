// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class ShiftOutCommand(NaplpsState state, List<byte> operands) : NaplpsCommand(state, SHIFT_OUT, operands)
{
}