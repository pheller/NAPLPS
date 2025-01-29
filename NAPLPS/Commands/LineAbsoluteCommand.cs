// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class LineAbsoluteCommand : LineCommand
{
    public LineAbsoluteCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, false, state, opcode, operands)
    {
    }
}