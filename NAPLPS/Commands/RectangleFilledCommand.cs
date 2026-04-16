// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Rectangle Filled", "Draw a filled rectangle with dimensions relative to the pen.", Category = CommandCategory.Geometric, DslKeyword = "rectFilled")]
public class RectangleFilledCommand : RectangleCommand
{
    public RectangleFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        ShouldFill = true;
    }
}