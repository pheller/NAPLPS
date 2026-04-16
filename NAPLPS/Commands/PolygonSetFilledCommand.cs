// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Polygon Set Filled", "Draw a filled polygon with absolute start and relative vertices.", Category = CommandCategory.Geometric, DslKeyword = "polySetFilled")]
public class PolygonSetFilledCommand : PolygonCommand
{
    public PolygonSetFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(true, state, opcode, operands)
    {
        ShouldFill = true;
    }
}