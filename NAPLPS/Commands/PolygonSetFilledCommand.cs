// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PolygonSetFilledCommand : PolygonSetCommand
{
    public PolygonSetFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, POLYGON_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}