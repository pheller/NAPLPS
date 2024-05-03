// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Diagnostics;

namespace NAPLPS.Commands;

/// <summary>Invokes G3MosaicSet into GL.</summary>
public class LockingShiftThreeCommand : EscCommand
{
    public LockingShiftThreeCommand(NaplpsState state, NaplpsOperands operands) : base(state, operands)
    {
        State.GL = NaplpsGSet.G3MosaicSet;
        State.InLockingManner = true;
         
        Debugger.Break();
    }
}