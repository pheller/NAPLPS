// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using System.Collections.Generic;
using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class RectangleFilledCommand : RectangleCommand
{
    public RectangleFilledCommand(List<byte> operands) : base(RECTANGLE_FILLED, operands)
    {
        ShouldFill = true;
    }
}