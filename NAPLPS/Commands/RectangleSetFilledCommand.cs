// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Rectangle Set Filled", "Will draw a filled reactangle from the specified point to the specified dimensions.")]
public class RectangleSetFilledCommand : RectangleSetCommand
{
    public RectangleSetFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, RECTANGLE_SET_FILLED, operands)
    {
        ShouldFill = true;
    }
}