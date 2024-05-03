// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Text;

namespace NAPLPS.Commands;

public class ShiftInCommand : NaplpsCommand
{
    public ShiftInCommand(NaplpsState state, NaplpsOperands operands) : base(state, SHIFT_IN, operands)
    {
        State.GL = NaplpsGSet.G0PrimarySet;
        State.InLockingManner = true;
    }

    public string Text => Encoding.ASCII.GetString(Operands.ToArray());
}