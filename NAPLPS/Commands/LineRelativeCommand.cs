// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class LineRelativeCommand : LineCommand
{
    public LineRelativeCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, true, state, opcode, operands)
    {

    }
}