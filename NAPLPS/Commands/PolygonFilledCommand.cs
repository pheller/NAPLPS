// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class PolygonFilledCommand : PolygonCommand
{
    public PolygonFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, POLYGON_FILLED, operands)
    {
        ShouldFill = true;
    }
}