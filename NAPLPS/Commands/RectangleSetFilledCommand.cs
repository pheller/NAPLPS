// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsCommands;

namespace NAPLPS.Commands;

public class RectangleSetFilledCommand : RectangleSetCommand
{
    public RectangleSetFilledCommand(NaplpsState state, List<byte> operands) : base(state, RECTANGLE_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}