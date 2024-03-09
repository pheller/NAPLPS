// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

public class RectangleFilledCommand : RectangleCommand
{
    public RectangleFilledCommand(NaplpsState state, NaplpsOperands operands) : base(state, RECTANGLE_FILLED, operands)
    {
        ShouldFill = true;
    }
}