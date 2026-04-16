// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

[AddCommand(200, "Polygon Filled", "Draw a filled polygon from relative vertices.", Category = CommandCategory.Geometric, DslKeyword = "polyFilled")]
public class PolygonFilledCommand : PolygonCommand
{
    public PolygonFilledCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(false, state, opcode, operands)
    {
        ShouldFill = true;
    }
}