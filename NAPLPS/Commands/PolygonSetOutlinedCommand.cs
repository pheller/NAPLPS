// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Polygon Set Outlined", "Draw an outlined polygon with absolute start and relative vertices.", Category = CommandCategory.Geometric, DslKeyword = "polySetOutlined")]
public class PolygonSetOutlinedCommand : PolygonCommand
{
    public PolygonSetOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(true, state, opcode, operands)
    {
        ShouldFill = false;
    }
}