// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>Invokes G3MosaicSet into GL.</summary>
public class LockingShiftThreeCommand : EscCommand
{
    public LockingShiftThreeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        //State.GLeft = 3;
        State.InLockingManner = true;

        State.RecordError(NaplpsErrorSeverity.Warning, NaplpsErrorType.InvalidCommand, "Locking shift three command encountered", opcode);
    }
}