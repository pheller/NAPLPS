// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Text;

namespace NAPLPS.Commands;

public class ShiftInCommand : GeometricDrawingCommandBase
{
    public ShiftInCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        State.DoShiftIn();
        State.InLockingManner = true;
    }
}