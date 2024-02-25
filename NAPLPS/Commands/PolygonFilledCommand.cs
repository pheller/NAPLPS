// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class PolygonFilledCommand : PolygonCommand
{
    public PolygonFilledCommand(List<byte> operands) : base(POLYGON_FILLED, operands)
    {
        ShouldFill = true;
    }
}