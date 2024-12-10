// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class RectangleSetOutlinedCommand : RectangleSetCommand
{
    public RectangleSetOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state, RECTANGLE_SET_OUTLINED, operands)
    {
        ShouldFill = false;
    }
}