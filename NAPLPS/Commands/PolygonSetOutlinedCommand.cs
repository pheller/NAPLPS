// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PolygonSetOutlinedCommand : PolygonCommand
{
    public PolygonSetOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state, POLYGON_SET_OUTLINED, operands)
    {
        ShouldFill = false;
    }
}