// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class RectangleOutlinedCommand : RectangleCommand
{
    public RectangleOutlinedCommand(NaplpsState state, NaplpsOperands operands) : base(state,RECTANGLE_OUTLINED, operands)
    {
        ShouldFill = false;
    }
}