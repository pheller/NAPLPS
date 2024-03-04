// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class RectangleOutlinedCommand : RectangleCommand
{
    public RectangleOutlinedCommand(NaplpsState state, List<byte> operands) : base(state,RECTANGLE_OUTLINED, operands)
    {
        ShouldOutline = true;
    }
}