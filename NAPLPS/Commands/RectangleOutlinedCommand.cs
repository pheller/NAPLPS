// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Rectangle Outlined", "Draw an outlined rectangle with dimensions relative to the pen.", Category = CommandCategory.Geometric, DslKeyword = "rectOutlined")]
public class RectangleOutlinedCommand : RectangleCommand
{
    public RectangleOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        ShouldFill = false;
    }
}