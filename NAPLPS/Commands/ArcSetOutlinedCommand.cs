// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Commands;

/// <summary>
/// The start point is the first block of coordinate data, specified in absolute coordinates.
/// The intermediate point is the second block of coordinate data, specified as a relative 
/// displacement from the start point, and the end point is the third block of coordinate data,
/// specified as a relative displacement from the intermediate point. The arc is not filled.
/// </summary>
[AddCommand(200, "Arc Set Outlined", "Draw an unfilled arc with absolute start, plus relative mid and end.", Category = CommandCategory.Geometric, DslKeyword = "arcSetOutlined")]
public class ArcSetOutlinedCommand : ArcCommand
{
    public ArcSetOutlinedCommand(NaplpsState state, byte opcode, NaplpsOperands operands) : base(true, state, opcode, operands)
    {
        ShouldFill = false;
    }
}