// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Diagnostics;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class PolygonSetFilledCommand : PolygonSetCommand
{
    public PolygonSetFilledCommand(NaplpsState state, List<byte> operands) : base(state, POLYGON_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}