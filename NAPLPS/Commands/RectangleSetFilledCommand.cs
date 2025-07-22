// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Rectangle Set Filled", "Will draw a filled reactangle from the specified point to the specified dimensions.")]
public class RectangleSetFilledCommand : RectangleSetCommand
{
    public RectangleSetFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        ShouldFill = true;
    }
}