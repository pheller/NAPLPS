// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>Invokes G3MosaicSet into GL.</summary>
public class LockingShiftThreeCommand : EscCommand
{
    public LockingShiftThreeCommand(NaplpsState state, NaplpsOperands operands) : base(state, operands)
    {
        State.GL = 3;
        State.InLockingManner = true;
         
        // Debugger.Break();
    }
}