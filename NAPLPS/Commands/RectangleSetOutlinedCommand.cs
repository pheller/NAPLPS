// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Rectangle Set Outlined", "Draw an outlined rectangle from a specified corner with given dimensions.", Category = CommandCategory.Geometric, DslKeyword = "rectSetOutlined")]
public class RectangleSetOutlinedCommand : RectangleSetCommand
{
    public RectangleSetOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(state, opcode, operands)
    {
        ShouldFill = false;
    }
}