// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class PolygonOutlinedCommand : PolygonCommand
{
    public PolygonOutlinedCommand(NaplpsState state, List<byte> operands) : base(state, POLYGON_OUTLINED, operands)
    {
        ShouldOutline = true;
    }
}