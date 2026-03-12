// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PolygonSetOutlinedCommand : PolygonCommand
{
    public PolygonSetOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(true, state, opcode, operands)
    {
        ShouldFill = false;
    }
}