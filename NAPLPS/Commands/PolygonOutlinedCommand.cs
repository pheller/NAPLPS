// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PolygonOutlinedCommand : PolygonCommand
{
    public PolygonOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state, POLYGON_OUTLINED, operands)
    {
        ShouldFill = false;
    }
}