// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Diagnostics;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class PolygonSetFilledCommand : PolygonSetCommand
{
    public PolygonSetFilledCommand(List<byte> operands) : base(POLYGON_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}