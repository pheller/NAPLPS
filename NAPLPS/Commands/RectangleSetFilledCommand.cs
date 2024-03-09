// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class RectangleSetFilledCommand : RectangleSetCommand
{
    public RectangleSetFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, RECTANGLE_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}