// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class LineAbsoluteCommand : LineCommand
{
    public LineAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, false, state, opcode, operands)
    {
    }
}