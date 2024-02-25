// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using System.Diagnostics;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class PolygonSetOutlinedCommand : PolygonSetCommand
{
    public PolygonSetOutlinedCommand(List<byte> operands) : base(POLYGON_SET_OUTLINED, operands)
    {
        ShouldOutline = true;
    }
}