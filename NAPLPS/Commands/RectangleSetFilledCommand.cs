// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class RectangleSetFilledCommand : RectangleSetCommand
{
    public RectangleSetFilledCommand(List<byte> operands) : base(RECTANGLE_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}