// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PolygonSetFilledCommand : PolygonCommand
{
    public PolygonSetFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(true, state, opcode, operands)
    {
        ShouldFill = true;
    }
}