// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Polygon Outlined", "Draw an outlined polygon from relative vertices.", Category = CommandCategory.Geometric, DslKeyword = "polyOutlined")]
public class PolygonOutlinedCommand : PolygonCommand
{
    public PolygonOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, state, opcode, operands)
    {
        ShouldFill = false;
    }
}