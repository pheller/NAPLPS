// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class ShiftOutCommand : NaplpsCommand
{
    public ShiftOutCommand(NaplpsState state, NaplpsOperands operands) : base(state, SHIFT_OUT, operands)
    {
        State.GL = NaplpsGSet.G1PDISet;
        State.InLockingManner = true;
    }
}