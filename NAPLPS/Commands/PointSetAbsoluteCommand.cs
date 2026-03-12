// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PointSetAbsoluteCommand : PointCommand
{
    public PointSetAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, state, opcode, operands)
    {
    }
}