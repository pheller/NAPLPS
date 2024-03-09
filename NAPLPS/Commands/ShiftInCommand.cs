// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Text;

namespace NAPLPS.Commands;

public class ShiftInCommand(NaplpsState state, NaplpsOperands operands) : NaplpsCommand(state, SHIFT_IN, operands)
{
    public string Text => Encoding.ASCII.GetString(Operands.ToArray());
}