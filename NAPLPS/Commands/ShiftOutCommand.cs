// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class ShiftOutCommand(NaplpsState state, NaplpsOperands operands) : NaplpsCommand(state, SHIFT_OUT, operands)
{
}