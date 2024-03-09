// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PolygonOutlinedCommand : PolygonCommand
{
    public PolygonOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state, POLYGON_OUTLINED, operands)
    {
        ShouldOutline = true;
    }
}