// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class RectangleOutlinedCommand : RectangleCommand
{
    public RectangleOutlinedCommand(List<byte> operands) : base(RECTANGLE_OUTLINED, operands)
    {
        ShouldOutline = true;
    }
}